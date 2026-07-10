const functions = require('firebase-functions');
const admin = require('firebase-admin');
const { GoogleGenerativeAI } = require('@google/generative-ai');
const axios = require('axios');

admin.initializeApp();
const db = admin.firestore();

const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY || '');
const model = genAI.getGenerativeModel({ model: 'gemini-2.0-flash' });

exports.processAIQuery = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { message, conversationId } = data;
  const userId = context.auth.uid;

  const history = [];
  if (conversationId) {
    const messagesSnap = await db.collection('conversations')
      .doc(conversationId)
      .collection('messages')
      .orderBy('createdAt', 'asc')
      .limit(20)
      .get();

    messagesSnap.forEach(doc => {
      const msg = doc.data();
      history.push({ role: msg.role === 'AI' ? 'model' : 'user', parts: [{ text: msg.content }] });
    });
  }

  const chat = model.startChat({ history });
  const result = await chat.sendMessage(message);
  const response = result.response.text();

  const convId = conversationId || db.collection('conversations').doc().id;
  const batch = db.batch();

  if (!conversationId) {
    batch.set(db.collection('conversations').doc(convId), {
      userId, title: message.substring(0, 100), status: 'active',
      createdAt: admin.firestore.FieldValue.serverTimestamp(),
      lastActivityAt: admin.firestore.FieldValue.serverTimestamp()
    });
  } else {
    batch.update(db.collection('conversations').doc(convId), {
      lastActivityAt: admin.firestore.FieldValue.serverTimestamp()
    });
  }

  batch.add(db.collection('conversations').doc(convId).collection('messages'), {
    role: 'User', content: message, createdAt: admin.firestore.FieldValue.serverTimestamp()
  });
  batch.add(db.collection('conversations').doc(convId).collection('messages'), {
    role: 'AI', content: response, createdAt: admin.firestore.FieldValue.serverTimestamp()
  });

  await batch.commit();

  return { conversationId: convId, reply: response };
});

exports.searchProducts = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { query, page = 1 } = data;

  try {
    const apiResponse = await axios.get(
      `https://commercehub-weou.onrender.com/api/v1/products/search`,
      { params: { q: query, page, pageSize: 20 } }
    );
    return apiResponse.data;
  } catch (error) {
    return { items: [], totalCount: 0, correctedQuery: query };
  }
});

exports.getRecommendations = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const userId = context.auth.uid;

  const recsSnap = await db.collection('recommendations')
    .where('userId', '==', userId)
    .orderBy('score', 'desc')
    .limit(10)
    .get();

  if (!recsSnap.empty) {
    return recsSnap.docs.map(d => d.data());
  }

  return [];
});

exports.onUserRegistered = functions.auth.user().onCreate(async (user) => {
  await db.collection('users').doc(user.uid).set({
    email: user.email,
    displayName: user.displayName || '',
    photoURL: user.photoURL || '',
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    preferences: { categories: [], priceRange: { min: 0, max: 10000 } }
  });
});
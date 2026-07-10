import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { AIAgentService, ChatResponse, Message } from '../../../../../../shared/src/lib/services/ai-agent.service';
import { AuthService } from '../../../../../../shared/src/lib/services/auth.service';

interface ChatMessage {
  id: string;
  role: 'user' | 'ai';
  content: string;
  timestamp: Date;
  recommendations?: any[];
  searchResults?: any[];
}

@Component({
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, OnDestroy {
  @ViewChild('chatMessages') chatMessagesRef!: ElementRef;
  @ViewChild('messageInput') messageInputRef!: ElementRef;

  isOpen = false;
  isMinimized = false;
  isTyping = false;
  messages: ChatMessage[] = [];
  currentMessage = '';
  conversationId?: number;
  unreadCount = 0;

  private welcomeMessage: ChatMessage = {
    id: 'welcome',
    role: 'ai',
    content: 'Hello! I\'m your AI Shopping Assistant. I can help you find products, get recommendations, track orders, and more! What would you like help with?',
    timestamp: new Date()
  };

  constructor(
    private aiAgent: AIAgentService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.messages.push(this.welcomeMessage);
  }

  ngOnDestroy(): void {
    // Cleanup
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.isMinimized = false;
      this.unreadCount = 0;
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  minimizeChat(): void {
    this.isMinimized = !this.isMinimized;
  }

  async sendMessage(): Promise<void> {
    const text = this.currentMessage.trim();
    if (!text || this.isTyping) return;

    this.currentMessage = '';

    this.messages.push({
      id: `user-${Date.now()}`,
      role: 'user',
      content: text,
      timestamp: new Date()
    });

    this.isTyping = true;
    this.scrollToBottom();

    try {
      const response = await this.aiAgent.chat({
        message: text,
        conversationId: this.conversationId
      }).toPromise();

      if (response.success && response.data) {
        this.conversationId = response.data.conversationId;

        this.messages.push({
          id: `ai-${Date.now()}`,
          role: 'ai',
          content: response.data.reply,
          timestamp: new Date(),
          recommendations: response.data.recommendations,
          searchResults: response.data.searchResults
        });

        if (!this.isOpen) {
          this.unreadCount++;
        }
      }
    } catch (error) {
      this.messages.push({
        id: `ai-${Date.now()}`,
        role: 'ai',
        content: 'Sorry, I encountered an error. Please try again.',
        timestamp: new Date()
      });
    }

    this.isTyping = false;
    this.scrollToBottom();
  }

  quickSearch(query: string): void {
    this.currentMessage = query;
    this.sendMessage();
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.chatMessagesRef) {
        const el = this.chatMessagesRef.nativeElement;
        el.scrollTop = el.scrollHeight;
      }
    }, 50);
  }
}
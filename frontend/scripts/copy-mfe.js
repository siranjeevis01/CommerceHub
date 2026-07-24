const fs = require('fs');
const path = require('path');

const distDir = path.resolve(__dirname, '..', 'dist');
const shellDist = path.join(distDir, 'shell');
const mfes = ['admin', 'vendor', 'storefront'];

console.log('=== MFE Copy Script ===');
console.log('Dist directory:', distDir);
console.log('Shell dist:', shellDist);

// Verify shell dist exists
if (!fs.existsSync(shellDist)) {
  console.error('ERROR: Shell dist directory does not exist:', shellDist);
  process.exit(1);
}

let allCopied = true;

for (const mfe of mfes) {
  const srcDir = path.join(distDir, mfe);
  const destDir = path.join(shellDist, mfe);

  if (!fs.existsSync(srcDir)) {
    console.error(`ERROR: MFE dist not found: ${srcDir}`);
    allCopied = false;
    continue;
  }

  // Check for remoteEntry.js
  const remoteEntry = path.join(srcDir, 'remoteEntry.js');
  if (!fs.existsSync(remoteEntry)) {
    console.error(`ERROR: remoteEntry.js not found in ${srcDir}`);
    allCopied = false;
    continue;
  }

  try {
    // Remove old copy if exists
    if (fs.existsSync(destDir)) {
      fs.rmSync(destDir, { recursive: true, force: true });
    }

    // Copy the entire MFE dist to shell dist
    fs.cpSync(srcDir, destDir, { recursive: true, force: true });

    // Verify copy
    const copiedRemoteEntry = path.join(destDir, 'remoteEntry.js');
    if (fs.existsSync(copiedRemoteEntry)) {
      console.log(`OK: Copied ${mfe} -> shell/${mfe} (remoteEntry.js verified)`);
    } else {
      console.error(`ERROR: Copy succeeded but remoteEntry.js missing at ${copiedRemoteEntry}`);
      allCopied = false;
    }

    // List files in the copied directory
    const files = fs.readdirSync(destDir).slice(0, 5);
    console.log(`  Files: ${files.join(', ')}${fs.readdirSync(destDir).length > 5 ? '...' : ''}`);
  } catch (err) {
    console.error(`ERROR copying ${mfe}:`, err.message);
    allCopied = false;
  }
}

if (!allCopied) {
  console.error('\nFAILED: One or more MFEs failed to copy');
  process.exit(1);
}

console.log('\nAll MFEs copied successfully to shell dist');

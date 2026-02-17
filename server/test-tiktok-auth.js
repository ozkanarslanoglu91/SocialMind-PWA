const BASE_URL = 'http://localhost:4000'

async function testAuthFlow() {
  console.log('🧪 Testing TikTok Authorization Flow...\n')
  
  try {
    // Step 1: Health check
    console.log('📝 Step 1: Health check...')
    const healthResponse = await fetch(`${BASE_URL}/api/health`)
    const healthData = await healthResponse.json()
    console.log('✅ Server is running:', healthData.status)
    console.log('\n')
    
    // Step 2: Get auth URL
    console.log('📝 Step 2: Getting authorization URL...')
    const authResponse = await fetch(`${BASE_URL}/api/social/tiktok/auth`)
    const authData = await authResponse.json()
    
    if (authData.success) {
      console.log('✅ Auth URL generated successfully!')
      console.log('✅ State:', authData.state)
      console.log('\n📋 Authorization URL:')
      console.log(authData.auth_url)
      console.log('\n')
      console.log('▶️  Next Steps:')
      console.log('   1. Copy the URL above')
      console.log('   2. Open it in your browser')
      console.log('   3. Authorize the app')
      console.log('   4. You will be redirected to: http://localhost:5173/auth/tiktok/callback?code=...')
      console.log('   5. The frontend will automatically exchange the code for a token\n')
    } else {
      console.error('❌ Failed to get auth URL:', authData.error)
    }
  } catch (error) {
    console.error('❌ Error:', error.message)
    console.log('\n💡 Make sure:')
    console.log('   1. Server is running: cd server && npm run dev')
    console.log('   2. .env file has TIKTOK_CLIENT_KEY and TIKTOK_CLIENT_SECRET')
    console.log('   3. Port 4000 is not in use\n')
  }
}

testAuthFlow()

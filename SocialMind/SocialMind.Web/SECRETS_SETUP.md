# SocialMind - Environment Configuration

## Setup

1. Copy `appsettings.secrets.template.json` to `appsettings.secrets.json`
2. Fill in your actual API keys and secrets
3. **Never commit `appsettings.secrets.json` to Git** (already in .gitignore)

## Environment Variables (Alternative)

Set these environment variables instead of using `appsettings.secrets.json`:

### Instagram/Facebook
```bash
SOCIALMIND_Instagram__AppId=your_app_id
SOCIALMIND_Instagram__AppSecret=your_app_secret
SOCIALMIND_Facebook__AppId=your_app_id
SOCIALMIND_Facebook__AppSecret=your_app_secret
```

### Stripe
```bash
SOCIALMIND_Stripe__PublishableKey=pk_test_xxx
SOCIALMIND_Stripe__SecretKey=sk_test_xxx
SOCIALMIND_Stripe__WebhookSecret=whsec_xxx
```

### Email
```bash
SOCIALMIND_Email__FromAddress=noreply@yourdomain.com
SOCIALMIND_Email__Username=your_email@gmail.com
SOCIALMIND_Email__Password=your_app_password
```

### AI APIs
```bash
SOCIALMIND_AI__OpenAI__ApiKey=sk-xxx
SOCIALMIND_AI__AzureOpenAI__ApiKey=xxx
SOCIALMIND_AI__AzureOpenAI__Endpoint=https://xxx.openai.azure.com/
SOCIALMIND_AI__GoogleGemini__ApiKey=xxx
```

## Azure Key Vault (Production)

For production, use Azure Key Vault:

```bash
# Install Azure CLI
az login

# Create Key Vault
az keyvault create --name socialmind-vault --resource-group socialmind-rg --location eastus

# Add secrets
az keyvault secret set --vault-name socialmind-vault --name "Instagram--AppId" --value "your_value"
az keyvault secret set --vault-name socialmind-vault --name "Stripe--SecretKey" --value "your_value"
```

Configure in Program.cs:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Security Best Practices

✅ **DO:**
- Use environment variables for local development
- Use Azure Key Vault for production
- Rotate secrets regularly
- Use separate keys for dev/staging/production
- Enable MFA on all service accounts

❌ **DON'T:**
- Commit secrets to Git
- Share secrets via email/chat
- Use production keys in development
- Store secrets in plain text files (except templates)
- Hard-code secrets in source code

## Instagram Setup

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app
3. Add Instagram Basic Display or Instagram Graph API
4. Configure OAuth redirect URLs
5. Get App ID and App Secret
6. Add to configuration

## Stripe Setup

1. Go to [Stripe Dashboard](https://dashboard.stripe.com/)
2. Get publishable and secret keys from Developers > API keys
3. Set up webhook endpoint: `https://yourdomain.com/api/webhooks/stripe`
4. Copy webhook signing secret
5. Add to configuration

## Testing

Use test credentials for development:
- Stripe: Use test mode keys (pk_test_*, sk_test_*)
- Instagram: Use test users from Facebook Developer Console

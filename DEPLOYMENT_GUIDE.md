# SocialMind Deployment Guide

## Production Deployment

### Prerequisites
- Docker installed
- Railway.app account (for Cloud deployment)
- Environment variables configured

### Local Docker Deployment

```bash
# Build Docker image
docker build -t socialmind:latest .

# Run container
docker run -p 5000:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=localhost;Database=socialmind;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;" \
  socialmind:latest
```

### Docker Compose

```bash
docker-compose up -d
```

### Railway Cloud Deployment

1. Connect GitHub repository to Railway.app
2. Configure environment variables in Railway dashboard:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<your-database-connection-string>
   SOCIALMIND_Instagram__AccessToken=<instagram-token>
   SOCIALMIND_YouTube__APIKey=<youtube-key>
   SOCIALMIND_TikTok__ClientKey=<tiktok-key>
   ```

3. Deploy:
   ```bash
   npm i -g @railway/cli
   railway login
   railway link <project-id>
   railway up
   ```

### Environment Variables

Create `appsettings.secrets.json` or set via environment:

```json
{
  "SocialMediaAPIs": {
    "Instagram": {
      "AccessToken": "igb_...",
      "BusinessAccountId": "..."
    },
    "YouTube": {
      "APIKey": "AIza...",
      "AccessToken": "ya29..."
    },
    "TikTok": {
      "ClientKey": "...",
      "ClientSecret": "...",
      "AccessToken": "..."
    }
  }
}
```

### Health Check

```bash
curl http://localhost:5000/health
```

### Production Considerations

1. **Database**: Use SQL Server or PostgreSQL in production
2. **HTTPS**: Enable HTTPS with proper certificates
3. **Authentication**: Configure proper OAuth providers
4. **Rate Limiting**: Implement API rate limiting
5. **Monitoring**: Set up Application Insights or similar
6. **Logging**: Enable comprehensive logging
7. **Backup**: Regular database backups

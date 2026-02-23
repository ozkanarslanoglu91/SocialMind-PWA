# SocialMind PWA - Progressive Web Application

## Overview

SocialMind is a **Progressive Web Application (PWA)** for AI-powered social media management with support for TikTok, YouTube, and Instagram.

## Features

### ✅ PWA Capabilities
- **Offline Support** - Service Worker caching strategy
- **Installable** - Add to home screen on mobile & desktop
- **App-Like Experience** - Standalone display mode
- **Fast Loading** - Stale-while-revalidate caching
- **Network Aware** - Different strategies for API vs static assets

### 🎯 Social Media Management
- Multi-platform post scheduling (TikTok, YouTube, Instagram)
- AI-powered content optimization
- Analytics & insights
- Real-time collaboration features

## Installation

### Local Development

```bash
# Clone the repository
git clone https://github.com/ozkanarslanoglu91/SocialMind-PWA.git
cd SocialMind-PWA

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Run tests
npm run test
```

### Install as PWA

1. **Desktop (Chrome, Edge, Safari)**
   - Click the install button in the address bar
   - Or: Menu → "Install app"

2. **Mobile (Android)**
   - Open in Chrome
   - Tap menu (⋮) → "Install app"

3. **iOS/iPadOS**
   - Open in Safari
   - Tap Share → "Add to Home Screen"

## Architecture

### Service Worker (`sw.js`)

The service worker implements multiple caching strategies:

```
API Requests     → Network first + Cache fallback
Static Assets    → Cache first with stale-while-revalidate
Navigation       → Fallback to index.html (SPA)
```

### Manifest Configuration (`manifest.json`)

- **name**: Full PWA name for splash screens
- **start_url**: Entry point (`/`)
- **display**: Standalone (app-like)
- **theme_color**: `#7c3aed` (purple)
- **icons**: 192x192 & 512x512 (maskable support)
- **shortcuts**: Quick actions for Dashboard, Create Post, Analytics

## Configuration

### PWA Meta Tags

Located in `index.html`:

```html
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
<meta name="apple-mobile-web-app-title" content="SocialMind">
<link rel="manifest" href="/manifest.json">
<link rel="apple-touch-icon" href="/pwa-192.png">
```

## Deployment

### GitHub Pages

Deployed automatically on push to `master` via GitHub Actions.

```
🔗 https://github.com/ozkanarslanoglu91/SocialMind-PWA
📦 Hosted at: https://socialmind.dev (after CNAME setup)
```

### Requirements

- Node.js 24+
- npm 11+
- Modern browser (Chrome, Edge, Firefox, Safari)

## Offline Support

SocialMind works offline with the following limitations:

- ✅ View cached content
- ✅ Draft posts
- ✅ Browse history
- ❌ Sync to platforms (requires internet)
- ❌ Fetch new analytics

When online, pending actions sync automatically.

## Performance

### Metrics

- **First Contentful Paint**: < 2s
- **Time to Interactive**: < 3.5s
- **Lighthouse Score**: 95+ (PWA category)

## Security

- ✅ HTTPS only
- ✅ Service Worker validation
- ✅ Content Security Policy headers
- ✅ Secure cookie flags

## Browser Support

| Browser | Desktop | Mobile |
|---------|---------|--------|
| Chrome  | ✅ Full | ✅ Full |
| Edge    | ✅ Full | ✅ Partial |
| Firefox | ✅ Full | ⚠️ Limited |
| Safari  | ⚠️ Partial | ✅ Full (iOS 16.4+) |

## Development

### Project Structure

```
SocialMind-PWA/
├── index.html              # PWA entry point
├── manifest.json           # Web app manifest
├── sw.js                   # Service Worker
├── .github/workflows/      # CI/CD pipelines
├── src/                    # Application source
└── dist/                   # Build output
```

### Available Scripts

```bash
npm run dev      # Start dev server
npm run build    # Production build
npm run preview  # Preview production build
npm run test     # Run tests
npm run lint     # Lint code
```

## Troubleshooting

### Service Worker Not Registering

1. Check browser console for errors
2. Ensure serving over HTTPS (or localhost)
3. Clear cache: DevTools → Application → Clear storage
4. Restart browser

### App Not Installing

- Manifest must be valid JSON
- Icons must be accessible
- HTTPS required (except localhost)
- Display mode must be "standalone"

### Offline Features Not Working

- Service Worker must be active (DevTools → Application → Service Workers)
- Check cache storage in DevTools
- Verify stale-while-revalidate is working

## Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Follow our code style
4. Submit a pull request

See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

MIT - See [LICENSE](LICENSE)

## Support

- 📧 Email: support@socialmind.dev
- 🐛 Issues: [GitHub Issues](https://github.com/ozkanarslanoglu91/SocialMind-PWA/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/ozkanarslanoglu91/SocialMind-PWA/discussions)

---

**Last Updated**: February 23, 2026
**Version**: 1.0.0
**Status**: ✅ Production Ready

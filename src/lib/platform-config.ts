import { Platform } from './types'

export const PLATFORM_CONFIG: Record<Platform, { name: string; color: string; maxLength: number }> = {
  youtube: { name: 'YouTube', color: 'oklch(0.55 0.20 25)', maxLength: 5000 },
  tiktok: { name: 'TikTok', color: 'oklch(0.70 0.18 190)', maxLength: 2200 },
  instagram: { name: 'Instagram', color: 'oklch(0.60 0.19 340)', maxLength: 2200 },
  facebook: { name: 'Facebook', color: 'oklch(0.50 0.15 250)', maxLength: 63206 },
  twitter: { name: 'Twitter/X', color: 'oklch(0.45 0.02 260)', maxLength: 280 },
  linkedin: { name: 'LinkedIn', color: 'oklch(0.50 0.15 230)', maxLength: 3000 },
}

export const getPlatformInitial = (platform: Platform): string => {
  return PLATFORM_CONFIG[platform].name.charAt(0)
}

export const formatNumber = (num: number): string => {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + 'M'
  }
  if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'K'
  }
  return num.toString()
}

export const getRelativeTime = (dateString: string): string => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins}m ago`
  if (diffHours < 24) return `${diffHours}h ago`
  if (diffDays < 7) return `${diffDays}d ago`
  return date.toLocaleDateString()
}

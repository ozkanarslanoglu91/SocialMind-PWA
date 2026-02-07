export type Platform = 'youtube' | 'tiktok' | 'instagram' | 'facebook' | 'twitter' | 'linkedin'

export type PostStatus = 'draft' | 'scheduled' | 'published' | 'failed'

export interface ConnectedAccount {
  id: string
  platform: Platform
  username: string
  avatarUrl?: string
  isActive: boolean
  connectedAt: string
}

export interface Post {
  id: string
  content: string
  platforms: Platform[]
  status: PostStatus
  scheduledFor?: string
  publishedAt?: string
  createdAt: string
  mediaUrl?: string
  analytics?: PostAnalytics
}

export interface PostAnalytics {
  views: number
  likes: number
  comments: number
  shares: number
  engagement: number
}

export interface AnalyticsMetric {
  label: string
  value: number
  change: number
  platform?: Platform
}

export interface AIGenerationParams {
  topic: string
  tone: 'professional' | 'casual' | 'humorous' | 'inspirational'
  platform?: Platform
  length: 'short' | 'medium' | 'long'
}

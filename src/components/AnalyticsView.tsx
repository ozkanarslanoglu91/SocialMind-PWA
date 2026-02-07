import { Post, Platform } from '@/lib/types'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { PlatformIcon } from '@/components/PlatformIcon'
import { PLATFORM_CONFIG, formatNumber } from '@/lib/platform-config'
import { TrendUp, TrendDown, Eye, Heart, ChatCircle, ShareNetwork, ChartBar } from '@phosphor-icons/react'
import { useState } from 'react'

interface AnalyticsViewProps {
  posts: Post[]
}

export function AnalyticsView({ posts }: AnalyticsViewProps) {
  const [selectedPlatform, setSelectedPlatform] = useState<Platform | 'all'>('all')
  const [timeRange, setTimeRange] = useState<'7d' | '30d' | '90d'>('30d')

  const filteredPosts = posts.filter(p => {
    if (p.status !== 'published') return false
    if (selectedPlatform === 'all') return true
    return p.platforms.includes(selectedPlatform)
  })

  const totalViews = filteredPosts.reduce((sum, p) => sum + (p.analytics?.views || 0), 0)
  const totalLikes = filteredPosts.reduce((sum, p) => sum + (p.analytics?.likes || 0), 0)
  const totalComments = filteredPosts.reduce((sum, p) => sum + (p.analytics?.comments || 0), 0)
  const totalShares = filteredPosts.reduce((sum, p) => sum + (p.analytics?.shares || 0), 0)
  const avgEngagement = filteredPosts.length > 0
    ? filteredPosts.reduce((sum, p) => sum + (p.analytics?.engagement || 0), 0) / filteredPosts.length
    : 0

  const topPosts = [...filteredPosts]
    .sort((a, b) => (b.analytics?.engagement || 0) - (a.analytics?.engagement || 0))
    .slice(0, 5)

  const platformPerformance: { platform: Platform; posts: number; avgEngagement: number }[] = []
  const platforms: Platform[] = ['youtube', 'tiktok', 'instagram', 'facebook', 'twitter', 'linkedin']
  
  platforms.forEach(platform => {
    const platformPosts = posts.filter(p => p.status === 'published' && p.platforms.includes(platform))
    if (platformPosts.length > 0) {
      const avgEng = platformPosts.reduce((sum, p) => sum + (p.analytics?.engagement || 0), 0) / platformPosts.length
      platformPerformance.push({ platform, posts: platformPosts.length, avgEngagement: avgEng })
    }
  })

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight mb-1">Analytics</h1>
          <p className="text-muted-foreground">Track your social media performance</p>
        </div>
        <div className="flex gap-3">
          <Select value={selectedPlatform} onValueChange={(v: any) => setSelectedPlatform(v)}>
            <SelectTrigger className="w-[160px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Platforms</SelectItem>
              {platforms.map(platform => (
                <SelectItem key={platform} value={platform}>
                  {PLATFORM_CONFIG[platform].name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={timeRange} onValueChange={(v: any) => setTimeRange(v)}>
            <SelectTrigger className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7d">Last 7 Days</SelectItem>
              <SelectItem value="30d">Last 30 Days</SelectItem>
              <SelectItem value="90d">Last 90 Days</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="glass-card p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Total Views</p>
              <p className="text-3xl font-bold mt-2" style={{ fontFamily: 'Space Grotesk' }}>
                {formatNumber(totalViews)}
              </p>
              <div className="flex items-center gap-1 mt-2 text-sm text-green-400">
                <TrendUp size={16} weight="bold" />
                <span>+12.5%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-accent/10">
              <Eye size={24} className="text-accent" weight="duotone" />
            </div>
          </div>
        </Card>

        <Card className="glass-card p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Total Likes</p>
              <p className="text-3xl font-bold mt-2" style={{ fontFamily: 'Space Grotesk' }}>
                {formatNumber(totalLikes)}
              </p>
              <div className="flex items-center gap-1 mt-2 text-sm text-green-400">
                <TrendUp size={16} weight="bold" />
                <span>+8.3%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-primary/10">
              <Heart size={24} className="text-primary" weight="duotone" />
            </div>
          </div>
        </Card>

        <Card className="glass-card p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Comments</p>
              <p className="text-3xl font-bold mt-2" style={{ fontFamily: 'Space Grotesk' }}>
                {formatNumber(totalComments)}
              </p>
              <div className="flex items-center gap-1 mt-2 text-sm text-green-400">
                <TrendUp size={16} weight="bold" />
                <span>+15.7%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-accent/10">
              <ChatCircle size={24} className="text-accent" weight="duotone" />
            </div>
          </div>
        </Card>

        <Card className="glass-card p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Avg Engagement</p>
              <p className="text-3xl font-bold mt-2" style={{ fontFamily: 'Space Grotesk' }}>
                {avgEngagement.toFixed(1)}%
              </p>
              <div className="flex items-center gap-1 mt-2 text-sm text-red-400">
                <TrendDown size={16} weight="bold" />
                <span>-2.1%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-primary/10">
              <ChartBar size={24} className="text-primary" weight="duotone" />
            </div>
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="p-6">
          <h2 className="text-xl font-semibold mb-4">Top Performing Posts</h2>
          {topPosts.length > 0 ? (
            <div className="space-y-4">
              {topPosts.map((post, index) => (
                <div key={post.id} className="flex gap-4 p-3 rounded-lg border border-border">
                  <div className="text-2xl font-bold text-muted-foreground" style={{ fontFamily: 'Space Grotesk' }}>
                    #{index + 1}
                  </div>
                  <div className="flex-1 space-y-2">
                    <div className="flex gap-2">
                      {post.platforms.map(platform => (
                        <PlatformIcon key={platform} platform={platform} size="sm" />
                      ))}
                    </div>
                    <p className="text-sm line-clamp-2">{post.content}</p>
                    <div className="flex gap-4 text-xs text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <Eye size={12} />
                        {formatNumber(post.analytics?.views || 0)}
                      </span>
                      <span className="flex items-center gap-1">
                        <Heart size={12} />
                        {formatNumber(post.analytics?.likes || 0)}
                      </span>
                      <Badge className="bg-accent/20 text-accent border-accent/30">
                        {post.analytics?.engagement.toFixed(1)}% engagement
                      </Badge>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              No published posts yet
            </div>
          )}
        </Card>

        <Card className="p-6">
          <h2 className="text-xl font-semibold mb-4">Platform Performance</h2>
          {platformPerformance.length > 0 ? (
            <div className="space-y-4">
              {platformPerformance
                .sort((a, b) => b.avgEngagement - a.avgEngagement)
                .map(({ platform, posts: postCount, avgEngagement }) => (
                  <div key={platform} className="space-y-2">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <PlatformIcon platform={platform} size="sm" />
                        <div>
                          <p className="font-medium">{PLATFORM_CONFIG[platform].name}</p>
                          <p className="text-xs text-muted-foreground">{postCount} posts</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-bold" style={{ fontFamily: 'Space Grotesk' }}>
                          {avgEngagement.toFixed(1)}%
                        </p>
                        <p className="text-xs text-muted-foreground">avg engagement</p>
                      </div>
                    </div>
                    <div className="h-2 bg-secondary rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-primary to-accent transition-all"
                        style={{ width: `${Math.min(avgEngagement * 10, 100)}%` }}
                      />
                    </div>
                  </div>
                ))}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              No platform data available
            </div>
          )}
        </Card>
      </div>
    </div>
  )
}

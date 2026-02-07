import { Post } from '@/lib/types'
import { PostCard } from '@/components/PostCard'
import { Card } from '@/components/ui/card'
import { TrendUp, Users, Eye, Heart } from '@phosphor-icons/react'
import { formatNumber } from '@/lib/platform-config'

interface DashboardViewProps {
  posts: Post[]
}

export function DashboardView({ posts }: DashboardViewProps) {
  const publishedPosts = posts.filter(p => p.status === 'published')
  const scheduledPosts = posts.filter(p => p.status === 'scheduled')
  
  const totalViews = publishedPosts.reduce((sum, p) => sum + (p.analytics?.views || 0), 0)
  const totalLikes = publishedPosts.reduce((sum, p) => sum + (p.analytics?.likes || 0), 0)
  const totalEngagement = publishedPosts.reduce((sum, p) => sum + (p.analytics?.engagement || 0), 0)
  const avgEngagement = publishedPosts.length > 0 ? totalEngagement / publishedPosts.length : 0

  const stats = [
    { label: 'Total Views', value: formatNumber(totalViews), icon: Eye, change: '+12.5%' },
    { label: 'Total Likes', value: formatNumber(totalLikes), icon: Heart, change: '+8.3%' },
    { label: 'Avg Engagement', value: avgEngagement.toFixed(1) + '%', icon: TrendUp, change: '+3.2%' },
    { label: 'Active Posts', value: publishedPosts.length.toString(), icon: Users, change: '+5' },
  ]

  const recentPosts = [...posts]
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, 6)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight mb-1">Dashboard</h1>
        <p className="text-muted-foreground">Overview of your social media performance</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat) => (
          <Card key={stat.label} className="p-6">
            <div className="flex items-start justify-between">
              <div>
                <p className="text-sm text-muted-foreground">{stat.label}</p>
                <p className="text-2xl font-bold mt-2" style={{ fontFamily: 'Space Grotesk' }}>
                  {stat.value}
                </p>
                <p className="text-xs text-green-400 mt-1">{stat.change}</p>
              </div>
              <div className="p-3 rounded-lg bg-primary/10">
                <stat.icon size={24} className="text-primary" weight="duotone" />
              </div>
            </div>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Recent Posts</h2>
            <span className="text-sm text-muted-foreground">{publishedPosts.length} published</span>
          </div>
          {recentPosts.length > 0 ? (
            <div className="space-y-3">
              {recentPosts.map((post) => (
                <PostCard key={post.id} post={post} />
              ))}
            </div>
          ) : (
            <Card className="p-8 text-center">
              <p className="text-muted-foreground">No posts yet. Create your first post to get started!</p>
            </Card>
          )}
        </div>

        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Scheduled</h2>
            <span className="text-sm text-muted-foreground">{scheduledPosts.length} upcoming</span>
          </div>
          {scheduledPosts.length > 0 ? (
            <div className="space-y-3">
              {scheduledPosts.map((post) => (
                <PostCard key={post.id} post={post} />
              ))}
            </div>
          ) : (
            <Card className="p-8 text-center">
              <p className="text-muted-foreground">No scheduled posts. Use the Smart Scheduler to plan ahead!</p>
            </Card>
          )}
        </div>
      </div>
    </div>
  )
}

import { Post } from '@/lib/types'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { PlatformIcon } from '@/components/PlatformIcon'
import { Calendar as CalendarIcon, Clock } from '@phosphor-icons/react'

interface ScheduleViewProps {
  posts: Post[]
}

export function ScheduleView({ posts }: ScheduleViewProps) {
  const scheduledPosts = posts
    .filter(p => p.status === 'scheduled' && p.scheduledFor)
    .sort((a, b) => new Date(a.scheduledFor!).getTime() - new Date(b.scheduledFor!).getTime())

  const groupPostsByDate = () => {
    const grouped: Record<string, Post[]> = {}
    scheduledPosts.forEach(post => {
      const date = new Date(post.scheduledFor!).toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
      if (!grouped[date]) grouped[date] = []
      grouped[date].push(post)
    })
    return grouped
  }

  const groupedPosts = groupPostsByDate()

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight mb-1">Schedule</h1>
        <p className="text-muted-foreground">Manage your scheduled posts</p>
      </div>

      {scheduledPosts.length === 0 ? (
        <Card className="p-12 text-center">
          <CalendarIcon size={64} className="mx-auto text-muted-foreground mb-4" weight="duotone" />
          <h3 className="text-xl font-semibold mb-2">No Scheduled Posts</h3>
          <p className="text-muted-foreground">
            Schedule posts to automatically publish at optimal times
          </p>
        </Card>
      ) : (
        <div className="space-y-8">
          {Object.entries(groupedPosts).map(([date, postsForDate]) => (
            <div key={date}>
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <CalendarIcon size={20} weight="duotone" />
                {date}
              </h2>
              <div className="space-y-3">
                {postsForDate.map(post => (
                  <Card key={post.id} className="p-5 hover:border-accent/50 transition-all">
                    <div className="flex gap-4">
                      <div className="flex flex-col items-center gap-2 min-w-[80px]">
                        <div className="text-center">
                          <div className="text-2xl font-bold" style={{ fontFamily: 'Space Grotesk' }}>
                            {new Date(post.scheduledFor!).toLocaleTimeString('en-US', {
                              hour: 'numeric',
                              minute: '2-digit',
                            })}
                          </div>
                          <Badge className="bg-blue-500/20 text-blue-400 border-blue-500/30 mt-1">
                            <Clock size={12} weight="fill" className="mr-1" />
                            Scheduled
                          </Badge>
                        </div>
                      </div>
                      
                      <div className="flex-1 space-y-3">
                        <div className="flex gap-2">
                          {post.platforms.map(platform => (
                            <PlatformIcon key={platform} platform={platform} size="sm" />
                          ))}
                        </div>
                        <p className="text-sm leading-relaxed">{post.content}</p>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Post } from '@/lib/types'
import { PlatformIcon } from './PlatformIcon'
import { formatNumber, getRelativeTime } from '@/lib/platform-config'
import { Eye, Heart, ChatCircle, ShareNetwork } from '@phosphor-icons/react'

interface PostCardProps {
  post: Post
}

export function PostCard({ post }: PostCardProps) {
  const getStatusColor = () => {
    switch (post.status) {
      case 'published': return 'bg-green-500/20 text-green-400 border-green-500/30'
      case 'scheduled': return 'bg-blue-500/20 text-blue-400 border-blue-500/30'
      case 'draft': return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30'
      case 'failed': return 'bg-red-500/20 text-red-400 border-red-500/30'
    }
  }

  return (
    <Card className="p-4 hover:border-accent/50 transition-all">
      <div className="space-y-3">
        <div className="flex items-start justify-between gap-3">
          <div className="flex gap-2">
            {post.platforms.map((platform) => (
              <PlatformIcon key={platform} platform={platform} size="sm" />
            ))}
          </div>
          <Badge className={getStatusColor()}>
            {post.status}
          </Badge>
        </div>

        <p className="text-sm leading-relaxed line-clamp-3">
          {post.content}
        </p>

        {post.analytics && (
          <div className="flex gap-4 text-xs text-muted-foreground">
            <span className="flex items-center gap-1">
              <Eye size={14} />
              {formatNumber(post.analytics.views)}
            </span>
            <span className="flex items-center gap-1">
              <Heart size={14} />
              {formatNumber(post.analytics.likes)}
            </span>
            <span className="flex items-center gap-1">
              <ChatCircle size={14} />
              {formatNumber(post.analytics.comments)}
            </span>
            <span className="flex items-center gap-1">
              <ShareNetwork size={14} />
              {formatNumber(post.analytics.shares)}
            </span>
          </div>
        )}

        <div className="text-xs text-muted-foreground">
          {post.status === 'scheduled' && post.scheduledFor && (
            <span>Scheduled for {new Date(post.scheduledFor).toLocaleString()}</span>
          )}
          {post.status === 'published' && post.publishedAt && (
            <span>Published {getRelativeTime(post.publishedAt)}</span>
          )}
          {post.status === 'draft' && (
            <span>Created {getRelativeTime(post.createdAt)}</span>
          )}
        </div>
      </div>
    </Card>
  )
}

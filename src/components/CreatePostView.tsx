import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { Platform, Post } from '@/lib/types'
import { PlatformSelector } from '@/components/PlatformSelector'
import { AIGeneratorDialog } from '@/components/AIGeneratorDialog'
import { Robot, Clock, PaperPlaneTilt } from '@phosphor-icons/react'
import { toast } from 'sonner'
import { PLATFORM_CONFIG } from '@/lib/platform-config'
import { Badge } from '@/components/ui/badge'

interface CreatePostViewProps {
  onCreatePost: (post: Omit<Post, 'id' | 'createdAt'>) => void
}

export function CreatePostView({ onCreatePost }: CreatePostViewProps) {
  const [content, setContent] = useState('')
  const [selectedPlatforms, setSelectedPlatforms] = useState<Platform[]>([])
  const [showAIDialog, setShowAIDialog] = useState(false)
  const [scheduledTime, setScheduledTime] = useState<string>('')

  const minCharLimit = Math.min(
    ...selectedPlatforms.map(p => PLATFORM_CONFIG[p].maxLength)
  )

  const handleAIGenerate = (generatedContent: string) => {
    setContent(generatedContent)
  }

  const handlePublish = () => {
    if (!content.trim()) {
      toast.error('Please enter some content')
      return
    }
    if (selectedPlatforms.length === 0) {
      toast.error('Please select at least one platform')
      return
    }

    onCreatePost({
      content,
      platforms: selectedPlatforms,
      status: 'published',
      publishedAt: new Date().toISOString(),
      analytics: {
        views: 0,
        likes: 0,
        comments: 0,
        shares: 0,
        engagement: 0,
      }
    })

    setContent('')
    setSelectedPlatforms([])
    toast.success('Post published successfully!')
  }

  const handleSchedule = () => {
    if (!content.trim()) {
      toast.error('Please enter some content')
      return
    }
    if (selectedPlatforms.length === 0) {
      toast.error('Please select at least one platform')
      return
    }
    if (!scheduledTime) {
      toast.error('Please select a time to schedule')
      return
    }

    onCreatePost({
      content,
      platforms: selectedPlatforms,
      status: 'scheduled',
      scheduledFor: new Date(scheduledTime).toISOString(),
    })

    setContent('')
    setSelectedPlatforms([])
    setScheduledTime('')
    toast.success('Post scheduled successfully!')
  }

  const handleSaveDraft = () => {
    if (!content.trim()) {
      toast.error('Please enter some content')
      return
    }

    onCreatePost({
      content,
      platforms: selectedPlatforms,
      status: 'draft',
    })

    setContent('')
    setSelectedPlatforms([])
    toast.success('Draft saved!')
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div>
        <h1 className="text-3xl font-bold tracking-tight mb-1">Create Post</h1>
        <p className="text-muted-foreground">Compose and share content across multiple platforms</p>
      </div>

      <Card className="p-6 space-y-6">
        <PlatformSelector
          selectedPlatforms={selectedPlatforms}
          onChange={setSelectedPlatforms}
        />

        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="content">Content</Label>
            <div className="flex items-center gap-3">
              {selectedPlatforms.length > 0 && (
                <Badge variant="outline" className="text-xs">
                  Max: {minCharLimit} chars
                </Badge>
              )}
              <span className={`text-sm ${content.length > minCharLimit ? 'text-destructive' : 'text-muted-foreground'}`}>
                {content.length} / {minCharLimit || '∞'}
              </span>
            </div>
          </div>
          <Textarea
            id="content"
            placeholder="What's on your mind? Share your thoughts with your audience..."
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={8}
            className="resize-none"
          />
          <Button
            variant="outline"
            onClick={() => setShowAIDialog(true)}
            className="w-full"
          >
            <Robot size={18} weight="duotone" />
            Generate with AI
          </Button>
        </div>

        <div className="space-y-2">
          <Label htmlFor="schedule-time">Schedule Time (Optional)</Label>
          <div className="flex gap-3">
            <input
              id="schedule-time"
              type="datetime-local"
              value={scheduledTime}
              onChange={(e) => setScheduledTime(e.target.value)}
              className="flex-1 px-3 py-2 rounded-md border border-input bg-background text-sm"
              min={new Date().toISOString().slice(0, 16)}
            />
            {scheduledTime && (
              <Button
                variant="ghost"
                onClick={() => setScheduledTime('')}
                size="sm"
              >
                Clear
              </Button>
            )}
          </div>
        </div>

        <div className="flex gap-3 pt-4">
          {scheduledTime ? (
            <Button onClick={handleSchedule} className="flex-1 gradient-button">
              <Clock size={18} weight="fill" />
              Schedule Post
            </Button>
          ) : (
            <Button onClick={handlePublish} className="flex-1 gradient-button">
              <PaperPlaneTilt size={18} weight="fill" />
              Publish Now
            </Button>
          )}
          <Button onClick={handleSaveDraft} variant="outline">
            Save Draft
          </Button>
        </div>
      </Card>

      <AIGeneratorDialog
        open={showAIDialog}
        onOpenChange={setShowAIDialog}
        onGenerate={handleAIGenerate}
      />
    </div>
  )
}

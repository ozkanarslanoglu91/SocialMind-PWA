import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { AIGenerationParams, Platform } from '@/lib/types'
import { Robot, Sparkle } from '@phosphor-icons/react'
import { toast } from 'sonner'

interface AIGeneratorDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onGenerate: (content: string) => void
  targetPlatform?: Platform
}

export function AIGeneratorDialog({ open, onOpenChange, onGenerate, targetPlatform }: AIGeneratorDialogProps) {
  const [params, setParams] = useState<AIGenerationParams>({
    topic: '',
    tone: 'professional',
    length: 'medium',
    platform: targetPlatform,
  })
  const [generating, setGenerating] = useState(false)
  const [suggestions, setSuggestions] = useState<string[]>([])

  const handleGenerate = async () => {
    if (!params.topic.trim()) {
      toast.error('Please enter a topic')
      return
    }

    setGenerating(true)
    try {
      const promptText = `Generate 3 different social media post variations about "${params.topic}". 
      Tone: ${params.tone}
      Length: ${params.length === 'short' ? '50-100 characters' : params.length === 'medium' ? '100-200 characters' : '200-300 characters'}
      ${params.platform ? `Platform: ${params.platform}` : ''}
      
      Make each variation engaging, authentic, and optimized for social media engagement. 
      Return as a JSON object with a "posts" array containing the 3 variations as strings.`
      
      const result = await window.spark.llm(promptText, 'gpt-4o-mini', true)
      const parsed = JSON.parse(result)
      setSuggestions(parsed.posts || [])
      toast.success('AI suggestions generated!')
    } catch (error) {
      toast.error('Failed to generate content. Please try again.')
      console.error(error)
    } finally {
      setGenerating(false)
    }
  }

  const handleSelect = (content: string) => {
    onGenerate(content)
    onOpenChange(false)
    setSuggestions([])
    setParams({ topic: '', tone: 'professional', length: 'medium', platform: targetPlatform })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Robot size={24} className="text-accent" weight="duotone" />
            AI Content Generator
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="topic">Topic or Theme</Label>
            <Textarea
              id="topic"
              placeholder="e.g., launching a new product, sharing industry insights, celebrating a milestone..."
              value={params.topic}
              onChange={(e) => setParams({ ...params, topic: e.target.value })}
              rows={3}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="tone">Tone</Label>
              <Select value={params.tone} onValueChange={(value: any) => setParams({ ...params, tone: value })}>
                <SelectTrigger id="tone">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="professional">Professional</SelectItem>
                  <SelectItem value="casual">Casual</SelectItem>
                  <SelectItem value="humorous">Humorous</SelectItem>
                  <SelectItem value="inspirational">Inspirational</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="length">Length</Label>
              <Select value={params.length} onValueChange={(value: any) => setParams({ ...params, length: value })}>
                <SelectTrigger id="length">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="short">Short</SelectItem>
                  <SelectItem value="medium">Medium</SelectItem>
                  <SelectItem value="long">Long</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {suggestions.length === 0 ? (
            <Button 
              onClick={handleGenerate} 
              disabled={generating || !params.topic.trim()}
              className="w-full gradient-button"
            >
              {generating ? (
                <>Generating...</>
              ) : (
                <>
                  <Sparkle size={18} weight="fill" />
                  Generate Content
                </>
              )}
            </Button>
          ) : (
            <div className="space-y-3">
              <Label>Select a suggestion or edit:</Label>
              {suggestions.map((suggestion, index) => (
                <div
                  key={index}
                  className="p-4 rounded-lg border-2 border-border bg-card hover:border-accent/50 cursor-pointer transition-all"
                  onClick={() => handleSelect(suggestion)}
                >
                  <p className="text-sm leading-relaxed">{suggestion}</p>
                </div>
              ))}
              <Button
                variant="outline"
                onClick={() => setSuggestions([])}
                className="w-full"
              >
                Generate Different Options
              </Button>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

import { Platform } from '@/lib/types'
import { PLATFORM_CONFIG } from '@/lib/platform-config'
import { PlatformIcon } from './PlatformIcon'
import { Switch } from '@/components/ui/switch'

interface PlatformSelectorProps {
  selectedPlatforms: Platform[]
  onChange: (platforms: Platform[]) => void
}

export function PlatformSelector({ selectedPlatforms, onChange }: PlatformSelectorProps) {
  const platforms: Platform[] = ['youtube', 'tiktok', 'instagram', 'facebook', 'twitter', 'linkedin']

  const togglePlatform = (platform: Platform) => {
    if (selectedPlatforms.includes(platform)) {
      onChange(selectedPlatforms.filter(p => p !== platform))
    } else {
      onChange([...selectedPlatforms, platform])
    }
  }

  return (
    <div className="space-y-3">
      <label className="text-sm font-semibold text-foreground">Select Platforms</label>
      <div className="grid grid-cols-2 gap-3">
        {platforms.map((platform) => {
          const isSelected = selectedPlatforms.includes(platform)
          return (
            <div
              key={platform}
              className={cn(
                'flex items-center gap-3 p-3 rounded-lg border-2 transition-all cursor-pointer',
                isSelected
                  ? 'border-accent bg-accent/10'
                  : 'border-border bg-card hover:border-muted-foreground/30'
              )}
              onClick={() => togglePlatform(platform)}
            >
              <PlatformIcon platform={platform} size="sm" />
              <span className="flex-1 text-sm font-medium">{PLATFORM_CONFIG[platform].name}</span>
              <Switch checked={isSelected} onCheckedChange={() => togglePlatform(platform)} />
            </div>
          )
        })}
      </div>
    </div>
  )
}

function cn(...classes: (string | boolean | undefined)[]) {
  return classes.filter(Boolean).join(' ')
}

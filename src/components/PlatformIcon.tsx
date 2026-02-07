import { Platform } from '@/lib/types'
import { PLATFORM_CONFIG, getPlatformInitial } from '@/lib/platform-config'
import { cn } from '@/lib/utils'

interface PlatformIconProps {
  platform: Platform
  size?: 'sm' | 'md' | 'lg'
  className?: string
}

export function PlatformIcon({ platform, size = 'md', className }: PlatformIconProps) {
  const config = PLATFORM_CONFIG[platform]
  
  const sizeClasses = {
    sm: 'w-6 h-6 text-xs',
    md: 'w-10 h-10 text-base',
    lg: 'w-14 h-14 text-lg',
  }

  return (
    <div
      className={cn(
        'platform-icon rounded-full flex items-center justify-center font-bold text-white',
        sizeClasses[size],
        className
      )}
      style={{ backgroundColor: config.color }}
    >
      {getPlatformInitial(platform)}
    </div>
  )
}

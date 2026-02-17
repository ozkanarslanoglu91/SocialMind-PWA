import { useState } from 'react'
import { useKV } from '@github/spark/hooks'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { Post } from '@/lib/types'
import { DashboardView } from '@/components/DashboardView'
import { CreatePostView } from '@/components/CreatePostView'
import { ScheduleView } from '@/components/ScheduleView'
import { AnalyticsView } from '@/components/AnalyticsView'
import { TikTokConnection } from '@/components/TikTokConnection'
import { Toaster } from '@/components/ui/sonner'
import { SquaresFour, PencilLine, Calendar, ChartBar, Plus } from '@phosphor-icons/react'

function App() {
  const [posts, setPosts] = useKV<Post[]>('socialmind-posts', [])
  const [activeTab, setActiveTab] = useState('dashboard')

  const postsArray = posts || []

  const handleCreatePost = (newPost: Omit<Post, 'id' | 'createdAt'>) => {
    setPosts((currentPosts) => [
      ...(currentPosts || []),
      {
        ...newPost,
        id: `post-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        createdAt: new Date().toISOString(),
      }
    ])
  }

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="fixed inset-0 -z-10 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary/20 via-background to-background" />
      
      <div className="container mx-auto px-4 py-6">
        <header className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold tracking-tight bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent">
                SocialMind
              </h1>
              <p className="text-sm text-muted-foreground mt-1">AI-Powered Social Media Hub</p>
            </div>
            {activeTab !== 'create' && (
              <Button
                onClick={() => setActiveTab('create')}
                className="gradient-button"
              >
                <Plus size={18} weight="bold" />
                Create Post
              </Button>
            )}
          </div>
        </header>

        <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
          <TabsList className="grid w-full grid-cols-5 lg:w-auto lg:inline-grid bg-card/50 backdrop-blur">
            <TabsTrigger value="dashboard" className="gap-2">
              <SquaresFour size={18} weight="duotone" />
              <span className="hidden sm:inline">Dashboard</span>
            </TabsTrigger>
            <TabsTrigger value="create" className="gap-2">
              <PencilLine size={18} weight="duotone" />
              <span className="hidden sm:inline">Create</span>
            </TabsTrigger>
            <TabsTrigger value="schedule" className="gap-2">
              <Calendar size={18} weight="duotone" />
              <span className="hidden sm:inline">Schedule</span>
            </TabsTrigger>
            <TabsTrigger value="analytics" className="gap-2">
              <ChartBar size={18} weight="duotone" />
              <span className="hidden sm:inline">Analytics</span>
            </TabsTrigger>
            <TabsTrigger value="tiktok" className="gap-2">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                <path d="M19.59 6.69a4.83 4.83 0 01-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 01-5.2 1.74 2.89 2.89 0 012.31-4.64 2.93 2.93 0 01.88.13V9.4a6.84 6.84 0 00-1-.05A6.33 6.33 0 005 20.1a6.34 6.34 0 0010.86-4.43v-7a8.16 8.16 0 004.77 1.52v-3.4a4.85 4.85 0 01-1-.1z"/>
              </svg>
              <span className="hidden sm:inline">TikTok</span>
            </TabsTrigger>
          </TabsList>

          <TabsContent value="dashboard">
            <DashboardView posts={postsArray} />
          </TabsContent>

          <TabsContent value="create">
            <CreatePostView onCreatePost={handleCreatePost} />
          </TabsContent>

          <TabsContent value="schedule">
            <ScheduleView posts={postsArray} />
          </TabsContent>

          <TabsContent value="tiktok">
            <TikTokConnection />
          </TabsContent>

          <TabsContent value="analytics">
            <AnalyticsView posts={postsArray} />
          </TabsContent>
        </Tabs>
      </div>

      <Toaster position="top-right" />
    </div>
  )
}

export default App
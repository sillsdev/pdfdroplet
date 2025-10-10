import { useCallback, useEffect, useMemo, useState, type DragEvent } from 'react'
import { bridge, type GenerationStatus, type LayoutMethodSummary, type PaperTargetInfo, type WorkspaceState } from './lib/bridge'

function toPreviewSrc(path: string | null | undefined) {
  if (!path) {
    return ''
  }

  const trimmed = path.trim()
  if (!trimmed) {
    return ''
  }

  if (/^[a-z]+:\/\//i.test(trimmed)) {
    return trimmed
  }

  const normalized = trimmed.replace(/\\/g, '/').replace(/^\/+/, '')
  return `file:///${normalized}`
}

function LayoutThumbnail({ summary }: { summary: LayoutMethodSummary }) {
  if (summary.thumbnailBase64) {
    return (
      <img
        src={`data:image/png;base64,${summary.thumbnailBase64}`}
        alt={`${summary.displayName} thumbnail`}
        className="h-16 w-16 rounded-lg border border-slate-200 bg-white object-contain"
      />
    )
  }

  return (
    <div className="flex h-16 w-16 items-center justify-center rounded-lg border border-slate-200 bg-white text-xs font-semibold text-slate-400">
      Preview
    </div>
  )
}

function App() {
  const [workspaceState, setWorkspaceState] = useState<WorkspaceState | null>(null)
  const [layouts, setLayouts] = useState<LayoutMethodSummary[]>([])
  const [paperTargets, setPaperTargets] = useState<PaperTargetInfo[]>([])
  const [generationStatus, setGenerationStatus] = useState<GenerationStatus | null>(null)
  const [isBootstrapping, setIsBootstrapping] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [isDragActive, setIsDragActive] = useState(false)

  useEffect(() => {
    let isMounted = true

    async function bootstrap() {
      try {
        const [state, layoutSummaries, paperSummaries] = await Promise.all([
          bridge.requestState(),
          bridge.requestLayouts(),
          bridge.requestPaperTargets(),
        ])

        if (!isMounted) {
          return
        }

        setWorkspaceState(state)
        setLayouts(layoutSummaries)
        setPaperTargets(paperSummaries)
      } catch (error) {
        console.error('Failed to bootstrap workspace bridge', error)
        if (error instanceof Error) {
          setErrorMessage(error.message)
        } else {
          setErrorMessage('Unable to load workspace information.')
        }
      } finally {
        if (isMounted) {
          setIsBootstrapping(false)
        }
      }
    }

    bootstrap()

    const unsubscribeState = bridge.on('stateChanged', (state) => {
      setWorkspaceState(state)
    })
    const unsubscribeLayouts = bridge.on('layoutsChanged', (list) => {
      setLayouts(list)
    })
    const unsubscribeGeneration = bridge.on('generationStatus', (status) => {
      setGenerationStatus(status)
    })
    const unsubscribePdfReady = bridge.on('generatedPdfReady', ({ path }) => {
      setWorkspaceState((current) => (current ? { ...current, generatedPdfPath: path } : current))
    })

    return () => {
      isMounted = false
      unsubscribeState()
      unsubscribeLayouts()
      unsubscribeGeneration()
      unsubscribePdfReady()
    }
  }, [])

  const runCommand = useCallback(async (command: () => Promise<WorkspaceState>) => {
    try {
      const updated = await command()
      setWorkspaceState(updated)
      setErrorMessage(null)
    } catch (error) {
      console.error('Workspace command failed', error)
      if (error instanceof Error) {
        setErrorMessage(error.message)
      } else {
        setErrorMessage('An unexpected error occurred while processing the command.')
      }
    }
  }, [])

  const handlePickPdf = useCallback(() => runCommand(() => bridge.pickPdf()), [runCommand])

  const handleReloadPrevious = useCallback(
    () => runCommand(() => bridge.reloadPrevious()),
    [runCommand],
  )

  const handleDrop = useCallback(
    (event: DragEvent<HTMLDivElement>) => {
      event.preventDefault()
      setIsDragActive(false)
      const file = event.dataTransfer.files?.[0]
      const path = (file as unknown as { path?: string })?.path

      if (!path) {
        setErrorMessage('Drag-and-drop requires the WebView host to supply a file path.')
        return
      }

      if (!path.toLowerCase().endsWith('.pdf')) {
        setErrorMessage('That file must have a .pdf extension.')
        return
      }

      setErrorMessage(null)
      void runCommand(() => bridge.dropPdf(path))
    },
    [runCommand],
  )

  const handleDragEnter = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setIsDragActive(true)
  }, [])

  const handleDragOver = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'copy'
  }, [])

  const handleDragLeave = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setIsDragActive(false)
  }, [])

  const handleSelectLayout = useCallback(
    (layoutId: string) => runCommand(() => bridge.setLayout(layoutId)),
    [runCommand],
  )

  const handleSelectPaper = useCallback(
    (paperId: string) => runCommand(() => bridge.setPaper(paperId)),
    [runCommand],
  )

  const handleToggleMirror = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setMirror(enabled)),
    [runCommand],
  )

  const handleToggleRtl = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setRightToLeft(enabled)),
    [runCommand],
  )

  const handleToggleCropMarks = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setCropMarks(enabled)),
    [runCommand],
  )

  const selectedPaperId = workspaceState?.selectedPaperId ?? (paperTargets[0]?.id ?? '')
  const selectedLayoutId = workspaceState?.selectedLayoutId ?? ''
  const hasIncomingPdf = Boolean(workspaceState?.hasIncomingPdf)
  const controlsDisabled = !workspaceState || !workspaceState.hasIncomingPdf
  const previewSrc = useMemo(() => toPreviewSrc(workspaceState?.generatedPdfPath), [workspaceState?.generatedPdfPath])
  const primaryMessage = useMemo(() => {
    if (generationStatus?.state === 'working') {
      return generationStatus.message || 'Preparing your booklet…'
    }

    return isDragActive ? 'Release to drop your PDF' : 'Drag a PDF document here'
  }, [generationStatus, isDragActive])

  return (
    <div className="flex min-h-screen gap-6 bg-droplet-background p-6 text-droplet-primary">
      <aside className="flex w-60 flex-col rounded-2xl bg-white p-4 shadow-panel">
        <label className="text-xs font-semibold uppercase tracking-wide text-slate-500">
          Printer Paper Size
        </label>
        <select
          className="mt-2 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 shadow-sm focus:border-droplet-accent focus:outline-none focus:ring-2 focus:ring-droplet-accent/20"
          value={selectedPaperId}
          onChange={(event) => handleSelectPaper(event.target.value)}
          disabled={isBootstrapping}
        >
          {paperTargets.map((paper) => (
            <option key={paper.id} value={paper.id}>
              {paper.displayName}
            </option>
          ))}
        </select>

        <div className="mt-4 flex-1 overflow-y-auto pr-1">
          <ul className="space-y-2">
            {layouts.map((layout) => {
              const isSelected = layout.id === selectedLayoutId
              return (
                <li key={layout.id}>
                  <button
                    className={`group flex w-full flex-col items-stretch rounded-xl border p-3 text-left transition ${
                      isSelected
                        ? 'border-droplet-accent bg-droplet-accent/10'
                        : 'border-transparent bg-slate-50 hover:border-slate-200 hover:bg-slate-100'
                    } ${layout.isEnabled ? 'cursor-pointer' : 'cursor-not-allowed opacity-60'}`}
                    disabled={!layout.isEnabled || isBootstrapping}
                    onClick={() => handleSelectLayout(layout.id)}
                  >
                    <div className="flex items-start gap-3">
                      <LayoutThumbnail summary={layout} />
                      <div>
                        <p className="text-sm font-semibold text-slate-800">{layout.displayName}</p>
                        <p className="mt-1 text-xs text-slate-500">
                          {layout.isOrientationSensitive ? 'Orientation sensitive' : 'Standard orientation'}
                        </p>
                      </div>
                    </div>
                  </button>
                </li>
              )
            })}
          </ul>
        </div>
      </aside>

      <main className="flex flex-1 flex-col gap-4">
        <section className="relative flex flex-1 items-stretch overflow-hidden rounded-2xl bg-white shadow-panel">
          {hasIncomingPdf && previewSrc ? (
            <iframe
              title="Booklet preview"
              src={previewSrc}
              className="h-full w-full border-0 bg-slate-100"
              allow="accelerometer; clipboard-write; encrypted-media; picture-in-picture"
            />
          ) : (
            <div
              className={`flex w-full flex-1 flex-col items-center justify-center gap-4 border-2 border-dashed ${
                isDragActive ? 'border-droplet-accent bg-droplet-accent/10' : 'border-slate-300 bg-white/90'
              } p-10 text-center`}
              onDragEnter={handleDragEnter}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
            >
              <div className="space-y-1">
                <p className="text-xl font-semibold text-slate-800">{primaryMessage}</p>
                <p className="text-sm text-slate-500">
                  Drop anywhere inside this window to get started or pick a PDF manually.
                </p>
              </div>
              <button
                type="button"
                disabled={isBootstrapping}
                onClick={handlePickPdf}
                className="rounded-lg bg-droplet-accent px-5 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-droplet-accent/90 disabled:cursor-not-allowed disabled:bg-slate-300"
              >
                Choose a PDF to open
              </button>
            </div>
          )}

          {generationStatus?.state === 'working' && (
            <div className="pointer-events-none absolute inset-0 flex items-end justify-end bg-slate-900/10">
              <div className="m-4 rounded-lg bg-slate-900/80 px-4 py-2 text-xs font-medium text-white">
                {generationStatus.message || 'Processing…'}
              </div>
            </div>
          )}
        </section>

        {errorMessage && (
          <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-2 text-sm text-red-700">
            {errorMessage}
          </div>
        )}

        <footer className="flex flex-col gap-3 rounded-2xl bg-white p-4 shadow-panel md:flex-row md:items-center md:justify-between">
          <div className="flex flex-wrap items-center gap-4 text-sm text-slate-700">
            <label className="inline-flex items-center gap-2">
              <input
                type="checkbox"
                className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
                checked={workspaceState?.rightToLeft ?? false}
                disabled={controlsDisabled}
                onChange={(event) => handleToggleRtl(event.target.checked)}
              />
              <span>Right-to-Left Language</span>
            </label>
            <label className="inline-flex items-center gap-2">
              <input
                type="checkbox"
                className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
                checked={workspaceState?.mirror ?? false}
                disabled={controlsDisabled}
                onChange={(event) => handleToggleMirror(event.target.checked)}
              />
              <span>Mirror</span>
            </label>
            <label className="inline-flex items-center gap-2">
              <input
                type="checkbox"
                className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
                checked={workspaceState?.showCropMarks ?? false}
                disabled={controlsDisabled}
                onChange={(event) => handleToggleCropMarks(event.target.checked)}
              />
              <span>Crop Marks</span>
            </label>
            {workspaceState?.canReloadPrevious && (
              <button
                type="button"
                onClick={handleReloadPrevious}
                className="text-sm font-semibold text-droplet-accent underline-offset-4 hover:underline disabled:text-slate-400"
                disabled={isBootstrapping}
              >
                Open Previous ({workspaceState.previousIncomingFilename})
              </button>
            )}
          </div>

          <div className="flex items-center gap-4 text-sm">
            <button
              type="button"
              onClick={handlePickPdf}
              className="text-droplet-accent underline-offset-4 hover:underline disabled:text-slate-400"
              disabled={isBootstrapping}
            >
              Choose a PDF to open
            </button>
            <button
              type="button"
              className="text-slate-500 underline-offset-4 hover:text-droplet-accent hover:underline"
            >
              Help
            </button>
          </div>
        </footer>
      </main>
    </div>
  )
}

export default App

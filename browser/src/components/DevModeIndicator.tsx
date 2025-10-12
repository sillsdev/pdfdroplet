import { useEffect, useState } from "react";
import { bridge, type RuntimeInfo } from "../lib/bridge";

export function DevModeIndicator() {
  const [runtimeInfo, setRuntimeInfo] = useState<RuntimeInfo | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function fetchRuntimeInfo() {
      try {
        const info = await bridge.getRuntimeInfo();
        if (isMounted) {
          setRuntimeInfo(info);
        }
      } catch (error) {
        console.warn("Failed to get runtime info", error);
      }
    }

    fetchRuntimeInfo();

    return () => {
      isMounted = false;
    };
  }, []);

  if (!runtimeInfo?.isDevMode) {
    return null;
  }

  let modeLabel: string;
  let modeColor: string;

  switch (runtimeInfo.mode) {
    case "bundle":
      modeLabel = ".net backend with bundled browser files";
      modeColor = "bg-blue-500";
      break;
    case "devServer":
      modeLabel = ".net backend with Vite Dev (HMR)";
      modeColor = "bg-green-500";
      break;
    case "stub":
      modeLabel = "Mock Backend";
      modeColor = "bg-purple-500";
      break;
  }

  return (
    <div className="fixed bottom-1 left-1 z-50">
      <div
        className={`${modeColor} text-white px-3 py-1.5 rounded-full text-xs font-semibold shadow-lg border-2 border-white/20 flex items-center gap-2`}
      >
        <div className="w-2 h-2 rounded-full bg-white/80 animate-pulse" />
        Mode: {modeLabel}
      </div>
    </div>
  );
}

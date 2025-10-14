import type { DragEvent } from "react";

function normalizeWindowsPath(path: string) {
  if (!path) {
    return "";
  }

  const trimmed = path.trim();
  if (!trimmed) {
    return "";
  }

  return trimmed.replace(/\//g, "\\");
}

function tryParseFileUri(value: string | null | undefined) {
  if (!value) {
    return null;
  }

  const trimmed = value.trim();
  if (!trimmed) {
    return null;
  }

  try {
    const url = new URL(trimmed);
    if (url.protocol !== "file:") {
      return null;
    }

    const decodedPath = decodeURIComponent(url.pathname || "");
    if (url.hostname) {
      const host = url.hostname;
      const uncPath = decodedPath.replace(/\//g, "\\");
      return `\\\\${host}${uncPath}`;
    }

    const withoutLeadingSlash = decodedPath.startsWith("/")
      ? decodedPath.slice(1)
      : decodedPath;
    return normalizeWindowsPath(withoutLeadingSlash);
  } catch (error) {
    console.debug("Failed to parse file URI", value, error);
    return null;
  }
}

export function extractDroppedPath(event: DragEvent<HTMLDivElement>) {
  const types = Array.from(event.dataTransfer?.types ?? []);
  let resolvedPath: string | null = null;

  console.groupCollapsed(
    `[drop] extractDroppedPath: types=${
      types.length > 0 ? types.join(", ") : "(none)"
    }`,
  );

  try {
    const parseCandidate = (candidate: string | null | undefined) => {
      if (!candidate) {
        return null;
      }

      const trimmed = candidate.replace(/\0/g, "").trim();
      if (!trimmed) {
        return null;
      }

      const uriParsed = tryParseFileUri(trimmed);
      if (uriParsed) {
        return uriParsed;
      }

      if (/^[a-zA-Z]:\\/.test(trimmed) || trimmed.startsWith("\\\\")) {
        return normalizeWindowsPath(trimmed);
      }

      return null;
    };

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const summaries = Array.from(files).map((file) => ({
        name: file.name,
        size: file.size,
        type: file.type,
      }));
      console.debug("[drop] FileList metadata", summaries);

      const explicitPath = (files[0] as unknown as { path?: string })?.path;
      if (explicitPath) {
        resolvedPath = normalizeWindowsPath(explicitPath);
        console.info("[drop] Resolved path from File.path", {
          raw: explicitPath,
          normalized: resolvedPath,
        });
      } else {
        console.debug(
          "[drop] Primary File object did not expose a path property",
        );
      }
    } else {
      console.debug("[drop] No File objects were present on the dataTransfer");
    }

    if (!resolvedPath) {
      const fileDrop = event.dataTransfer.getData("FileDrop");
      if (fileDrop) {
        const preview =
          fileDrop.length > 256 ? `${fileDrop.slice(0, 256)}…` : fileDrop;
        console.debug("[drop] FileDrop payload preview", preview);

        const lines = fileDrop
          .split(/\0|\r?\n/)
          .map((line) => line.trim())
          .filter((line) => line.length > 0);

        for (const line of lines) {
          const parsed = parseCandidate(line);
          console.debug("[drop] Evaluating FileDrop line", { line, parsed });
          if (parsed) {
            resolvedPath = parsed;
            break;
          }
        }
      } else {
        console.debug("[drop] No FileDrop entry found on dataTransfer");
      }
    }

    if (!resolvedPath) {
      const uriList = event.dataTransfer.getData("text/uri-list");
      if (uriList) {
        const preview =
          uriList.length > 256 ? `${uriList.slice(0, 256)}…` : uriList;
        console.debug("[drop] text/uri-list payload preview", preview);

        const lines = uriList
          .split(/\r?\n/)
          .map((line) => line.trim())
          .filter((line) => line.length > 0 && !line.startsWith("#"));

        for (const line of lines) {
          const parsed = parseCandidate(line);
          console.debug("[drop] Evaluating URI list line", { line, parsed });
          if (parsed) {
            resolvedPath = parsed;
            break;
          }
        }
      } else {
        console.debug("[drop] No text/uri-list payload present");
      }
    }

    if (!resolvedPath) {
      const plainText = event.dataTransfer.getData("text/plain");
      if (plainText) {
        const preview =
          plainText.length > 256 ? `${plainText.slice(0, 256)}…` : plainText;
        console.debug("[drop] text/plain payload preview", preview);
      }

      const parsedPlain = parseCandidate(plainText);
      if (parsedPlain) {
        console.debug(
          "[drop] Resolved path from text/plain entry",
          parsedPlain,
        );
        resolvedPath = parsedPlain;
      }
    }
  } finally {
    console.info("[drop] extractDroppedPath resolved path", resolvedPath);
    console.groupEnd();
  }

  return resolvedPath;
}

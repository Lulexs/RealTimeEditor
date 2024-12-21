import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { useEffect } from "react";

interface LoadingTrackerPluginProps {
  onFirstContentLoad: () => void;
}

export default function LoadingTrackerPlugin({
  onFirstContentLoad,
}: LoadingTrackerPluginProps): null {
  const [editor] = useLexicalComposerContext();

  useEffect(() => {
    return editor.registerUpdateListener(({ editorState }) => {
      if (!editorState.isEmpty()) {
        onFirstContentLoad();
      }
    });
  }, [editor, onFirstContentLoad]);

  return null;
}

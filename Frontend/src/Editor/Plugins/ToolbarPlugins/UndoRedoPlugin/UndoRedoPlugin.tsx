import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { UNDO_COMMAND, REDO_COMMAND } from "lexical";
import classes from "./UndoRedoPlugin.module.css";
import { IconArrowBack, IconArrowForward } from "@tabler/icons-react";

export default function UndoRedoPlugin() {
  const [editor] = useLexicalComposerContext();

  const onClick = (command: "undo" | "redo"): void => {
    if (command === "undo") {
      editor.dispatchCommand(UNDO_COMMAND, undefined);
      return;
    }
    editor.dispatchCommand(REDO_COMMAND, undefined);
  };

  return (
    <>
      <button className={classes.toolbarButton} onClick={() => onClick("undo")}>
        <IconArrowBack />
      </button>
      <button className={classes.toolbarButton} onClick={() => onClick("redo")}>
        <IconArrowForward />
      </button>
    </>
  );
}

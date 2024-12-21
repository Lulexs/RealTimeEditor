import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import {
  INSERT_ORDERED_LIST_COMMAND,
  INSERT_UNORDERED_LIST_COMMAND,
} from "@lexical/list";
import classes from "./ListPlugin.module.css";
import { IconList, IconListNumbers } from "@tabler/icons-react";

export default function ListPlugin() {
  const [editor] = useLexicalComposerContext();

  const onClick = (tag: "ul" | "ol"): void => {
    if (tag === "ol") {
      editor.dispatchCommand(INSERT_ORDERED_LIST_COMMAND, undefined);
      return;
    }
    editor.dispatchCommand(INSERT_UNORDERED_LIST_COMMAND, undefined);
  };

  return (
    <>
      <button className={classes.toolbarButton} onClick={() => onClick("ul")}>
        <IconList />
      </button>
      <button className={classes.toolbarButton} onClick={() => onClick("ol")}>
        <IconListNumbers />
      </button>
    </>
  );
}

import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import {
  IconAlignCenter,
  IconAlignJustified,
  IconAlignLeft,
  IconAlignRight,
} from "@tabler/icons-react";
import { FORMAT_ELEMENT_COMMAND } from "lexical";
import classes from "./AlignPlugin.module.css";

type alignment = "left" | "right" | "center" | "justify";

export default function AlignPlugin() {
  const [editor] = useLexicalComposerContext();

  const getIcon = (align: alignment): JSX.Element | null => {
    switch (align) {
      case "left":
        return <IconAlignLeft />;
      case "right":
        return <IconAlignRight />;
      case "center":
        return <IconAlignCenter />;
      case "justify":
        return <IconAlignJustified />;
      default:
        return null;
    }
  };

  const onClick = (align: alignment): void => {
    editor.dispatchCommand(FORMAT_ELEMENT_COMMAND, align);
  };

  const alignments: alignment[] = ["left", "right", "center", "justify"];

  return (
    <>
      {alignments.map((alignment) => (
        <button
          key={alignment}
          className={classes.toolbarButton}
          onClick={() => onClick(alignment)}
        >
          {getIcon(alignment)}
        </button>
      ))}
    </>
  );
}

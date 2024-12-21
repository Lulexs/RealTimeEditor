import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import {
  IconH1,
  IconH2,
  IconH3,
  IconH4,
  IconH5,
  IconH6,
} from "@tabler/icons-react";
import { $getSelection, $isRangeSelection } from "lexical";
import { $setBlocksType } from "@lexical/selection";
import { $createHeadingNode } from "@lexical/rich-text";
import classes from "./HeadingPlugin.module.css";

type HeadingType = "h1" | "h2" | "h3" | "h4" | "h5" | "h6";

export default function HeadingPlugin() {
  const [editor] = useLexicalComposerContext();

  const getIcon = (heading: HeadingType): JSX.Element | null => {
    switch (heading) {
      case "h1":
        return <IconH1 />;
      case "h2":
        return <IconH2 />;
      case "h3":
        return <IconH3 />;
      case "h4":
        return <IconH4 />;
      case "h5":
        return <IconH5 />;
      case "h6":
        return <IconH6 />;
      default:
        return null;
    }
  };

  const onClick = (heading: HeadingType): void => {
    editor.update(() => {
      const selection = $getSelection();
      if ($isRangeSelection(selection)) {
        $setBlocksType(selection, () => $createHeadingNode(heading));
      }
    });
  };

  const headings: HeadingType[] = ["h1", "h2", "h3", "h4", "h5", "h6"];

  return (
    <>
      {headings.map((heading) => (
        <button
          key={heading}
          className={classes.toolbarButton}
          onClick={() => onClick(heading)}
        >
          {getIcon(heading)}
        </button>
      ))}
    </>
  );
}

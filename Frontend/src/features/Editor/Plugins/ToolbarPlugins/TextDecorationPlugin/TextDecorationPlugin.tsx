import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import {
  IconBold,
  IconItalic,
  IconStrikethrough,
  IconUnderline,
} from "@tabler/icons-react";
import { $getSelection, $isRangeSelection, TextFormatType } from "lexical";
import classes from "./TextDecorationPlugin.module.css";

export default function TextDecorationPlugin() {
  const [editor] = useLexicalComposerContext();

  const getIcon = (format: TextFormatType): JSX.Element | null => {
    switch (format) {
      case "bold":
        return <IconBold />;
      case "italic":
        return <IconItalic />;
      case "strikethrough":
        return <IconStrikethrough />;
      case "underline":
        return <IconUnderline />;
      default:
        return null;
    }
  };

  const onClick = (format: TextFormatType): void => {
    editor.update(() => {
      const selection = $getSelection();
      if ($isRangeSelection(selection)) {
        selection.formatText(format);
      }
    });
  };

  const supportedTextFormats: TextFormatType[] = [
    "bold",
    "italic",
    "strikethrough",
    "underline",
  ];
  return (
    <>
      {supportedTextFormats.map((format) => (
        <button
          key={format}
          className={classes.toolbarButton}
          onClick={() => onClick(format)}
        >
          {getIcon(format)}
        </button>
      ))}
    </>
  );
}

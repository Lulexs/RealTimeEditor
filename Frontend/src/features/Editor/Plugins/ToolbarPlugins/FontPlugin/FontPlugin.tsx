import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { $patchStyleText } from "@lexical/selection";
import { $getSelection } from "lexical";
import classes from "./FontPlugin.module.css";

const FONT_FAMILY_OPTIONS: [string, string][] = [
  ["Arial", "Arial"],
  ["Courier New", "Courier New"],
  ["Georgia", "Georgia"],
  ["Times New Roman", "Times New Roman"],
  ["Trebuchet MS", "Trebuchet MS"],
  ["Verdana", "Verdana"],
];

const FONT_SIZE_OPTIONS: [string, string][] = [
  ["10pt", "10px"],
  ["11pt", "11px"],
  ["12pt", "12px"],
  ["13pt", "13px"],
  ["14pt", "14px"],
  ["15pt", "15px"],
  ["16pt", "16px"],
  ["17pt", "17px"],
  ["18pt", "18px"],
  ["19pt", "19px"],
  ["20pt", "20px"],
];

export default function FontPlugin() {
  const [editor] = useLexicalComposerContext();

  const onChangeStyle = (style: string, option: string) => {
    editor.update(() => {
      const selection = $getSelection();
      if (selection !== null) {
        $patchStyleText(selection, { [style]: option });
      }
    });
  };

  return (
    <>
      <select
        className={`${classes.fontSelect} ${classes.fontFamilySelect}`}
        onChange={(e) => onChangeStyle("font-family", e.target.value)}
      >
        {FONT_FAMILY_OPTIONS.map(([option, text]) => (
          <option key={option} value={option}>
            {text}
          </option>
        ))}
      </select>
      <select
        className={`${classes.fontSelect} ${classes.fontSizeSelect}`}
        onChange={(e) => onChangeStyle("font-size", e.target.value)}
      >
        {FONT_SIZE_OPTIONS.map(([option, text]) => (
          <option key={option} value={option}>
            {text}
          </option>
        ))}
      </select>
    </>
  );
}

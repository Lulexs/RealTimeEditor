import React from "react";
import TextDecorationPlugin from "./TextDecorationPlugin/TextDecorationPlugin";
import classes from "./ToolbarPlugin.module.css";
import HeadingPlugin from "./HeadingPlugin/HeadingPlugin";
import ListPlugin from "./ListPlugin/ListPlugin";
import AlignPlugin from "./AlignPlugin/AlignPlugin";
import FontPlugin from "./FontPlugin/FontPlugin";
import UndoRedoPlugin from "./UndoRedoPlugin/UndoRedoPlugin";
import ActiveUsersPlugin from "./ActiveUsersPlugin/ActiveUsersPlugin";
import { UserProfile } from "../../Interfaces/UserProfile";

export interface ActiveUsersPluginInterface {
  myProfile: UserProfile;
  otherUsers: UserProfile[];
}

export default function ToolbarPlugin(
  props: ActiveUsersPluginInterface
): JSX.Element {
  const plugins = [
    { label: "Font", component: FontPlugin },
    { label: "History", component: UndoRedoPlugin },
    {
      label: "Decorations",
      component: TextDecorationPlugin,
    },
    {
      label: "Headings",
      component: HeadingPlugin,
    },
    {
      label: "Lists",
      component: ListPlugin,
    },
    { label: "Alignments", component: AlignPlugin },
  ];

  return (
    <div className={classes.toolbarContainer}>
      {plugins.map((plugin, idx) => (
        <div
          className={`${classes.toolbarGroup} ${
            idx != plugins.length - 1 ? classes.borderRight : ""
          }`}
          key={plugin.label}
        >
          <div className={classes.toolbarDescription}>{plugin.label}</div>
          <div>{React.createElement(plugin.component)}</div>
        </div>
      ))}
      <ActiveUsersPlugin
        myProfile={props.myProfile}
        otherUsers={props.otherUsers}
      />
    </div>
  );
}

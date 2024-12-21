import { LexicalComposer } from "@lexical/react/LexicalComposer";
import { ContentEditable } from "@lexical/react/LexicalContentEditable";
import { LexicalErrorBoundary } from "@lexical/react/LexicalErrorBoundary";
import { RichTextPlugin } from "@lexical/react/LexicalRichTextPlugin";
import classes from "./Editor.module.css";
import ToolbarPlugin from "./Plugins/ToolbarPlugins/ToolbarPlugin";
import { HeadingNode } from "@lexical/rich-text";
import { ListNode, ListItemNode } from "@lexical/list";
import { ListPlugin } from "@lexical/react/LexicalListPlugin";
import { CollaborationPlugin } from "@lexical/react/LexicalCollaborationPlugin";
import { Provider } from "@lexical/yjs";
import * as Y from "yjs";
import { WebsocketProvider } from "y-websocket";
import { getRandomUserProfile } from "../getRandomUserProfile";
import { useCallback, useEffect, useState } from "react";
import { theme } from "./Theming/EditorThemes";
import { ActiveUserProfile } from "./Interfaces/ActiveUserProfile";
import "./Theming/theming.css";
import { notifications } from "@mantine/notifications";
import LoadingTrackerPlugin from "./Plugins/LoadingTrackerPlugin";
import DocumentHeader from "./DocumentHeader";

function getDocFromMap(id: string, yjsDocMap: Map<string, Y.Doc>): Y.Doc {
  let doc = yjsDocMap.get(id);

  if (doc === undefined) {
    doc = new Y.Doc();
    yjsDocMap.set(id, doc);
  } else {
    doc.load();
  }

  return doc;
}

const initialConfig = {
  namespace: "MyEditor",
  theme: theme,
  onError: (error: Error) => console.log(error),
  nodes: [HeadingNode, ListNode, ListItemNode],
  editorState: null,
};

export interface EditorProps {
  workspaceId: string;
  documentId: string;
  createdAt: Date;
  ownerUsername: string;
  documentName: string;
  snapshots: { [key: string]: Date };
}

const defaultProps: EditorProps = {
  workspaceId: "bb4f9ca1-41ec-469c-bbc8-666666666666",
  documentId: "0d49e653-9d02-4339-98a2-f122222425b2",
  createdAt: new Date(),
  ownerUsername: "TestUser",
  documentName: "Sample Document",
  snapshots: {
    snapshot1: new Date(),
    snapshot2: new Date(),
  },
};

export default function Editor(props: EditorProps = defaultProps) {
  const [userProfile] = useState(() => getRandomUserProfile());
  const [activeUsers, setActiveUsers] = useState<ActiveUserProfile[]>([]);
  const [yjsProvider, setYjsProvider] = useState<null | Provider>(null);
  const [providerName] = useState("websocket");
  const [connectionStatus, setConnectionStatus] = useState<
    "disconnected" | "connecting" | "connected" | null
  >(null);
  const [contentLoaded, setContentLoaded] = useState(false);

  const handleAwarenessUpdate = useCallback(() => {
    const awareness = yjsProvider?.awareness;
    setActiveUsers(
      Array.from(awareness ? awareness.getStates().entries() : []).map(
        ([userId, { color, name }]) => ({
          color,
          name,
          userId,
        })
      )
    );
  }, [yjsProvider]);

  useEffect(() => {
    if (yjsProvider == null) return;
    yjsProvider.awareness.on("update", handleAwarenessUpdate);

    return () => yjsProvider.awareness.off("update", handleAwarenessUpdate);
  }, [yjsProvider, handleAwarenessUpdate]);

  const createProvider = useCallback(
    (id: string, yjsDocMap: Map<string, Y.Doc>): Provider => {
      const doc = getDocFromMap(id, yjsDocMap);

      const provider = new WebsocketProvider("http://localhost:5287", id, doc, {
        connect: true,
      });

      provider.on("connection-close", (e) => {
        if (e != null) {
          provider.shouldConnect = false;
          notifications.show({
            title: "Connection failed",
            message: e?.reason,
            autoClose: false,
            color: "red",
          });
          notifications.show({
            title: "Connection failed",
            message: e?.reason,
            autoClose: false,
            color: "red",
          });
        }
      });

      provider.on("status", (s) => {
        setConnectionStatus(s.status);
        if (s.status == "connected") {
          notifications.show({
            title: "Preparing document",
            message: "Please wait while we prepare document's content",
            loading: true,
          });
        }
      });

      // @ts-ignore
      setTimeout(() => setYjsProvider(provider), 0);

      // @ts-ignore
      return provider;
    },
    [providerName]
  );

  return (
    <>
      <DocumentHeader
        {...{
          workspaceId: props.workspaceId ?? defaultProps.workspaceId,
          documentId: props.documentId ?? defaultProps.documentId,
          createdAt: props.createdAt ?? defaultProps.createdAt,
          ownerUsername: props.ownerUsername ?? defaultProps.ownerUsername,
          documentName: props.documentName ?? defaultProps.documentName,
          snapshots: props.snapshots ?? defaultProps.snapshots,
          connectionStatus:
            connectionStatus != "connected" && connectionStatus != null
              ? connectionStatus
              : contentLoaded
              ? "connected"
              : "connecting",
        }}
      />
      <LexicalComposer initialConfig={initialConfig}>
        <LoadingTrackerPlugin
          onFirstContentLoad={() => {
            notifications.clean();
            setContentLoaded(true);
          }}
        />
        <CollaborationPlugin
          id="ws/bb4f9ca1-41ec-469c-bbc8-666666666666/0d49e653-9d02-4339-98a2-f122222425b2"
          shouldBootstrap={false}
          providerFactory={createProvider}
          username={userProfile.name}
          cursorColor={userProfile.color}
        />
        <ToolbarPlugin myProfile={userProfile} otherUsers={activeUsers} />
        <ListPlugin />
        <div className={classes.editorContainer}>
          <RichTextPlugin
            contentEditable={
              <ContentEditable className={classes.editorContent} />
            }
            ErrorBoundary={LexicalErrorBoundary}
          />
        </div>
      </LexicalComposer>
    </>
  );
}

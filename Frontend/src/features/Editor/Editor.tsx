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
import { useCallback, useEffect, useState, useMemo } from "react";
import { theme } from "./Theming/EditorThemes";
import { ActiveUserProfile } from "./Interfaces/ActiveUserProfile";
import "./Theming/theming.css";
import { notifications } from "@mantine/notifications";
import LoadingTrackerPlugin from "./Plugins/LoadingTrackerPlugin";
import DocumentHeader from "./DocumentHeader";
import { observer } from "mobx-react-lite";
import { useStore } from "../../app/stores/store";

const avatarToColor = new Map(
  Object.entries({
    "1": "#FF5733",
    "2": "#33FF57",
    "3": "#3357FF",
    "4": "#FFFF33",
    "5": "#FF33FF",
    "6": "#33FFFF",
    "7": "#FF9933",
    "8": "#9933FF",
    "9": "#33FF99",
  })
);

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

export default observer(function Editor() {
  const [activeUsers, setActiveUsers] = useState<ActiveUserProfile[]>([]);
  const [yjsProvider, setYjsProvider] = useState<null | Provider>(null);
  const [connectionStatus, setConnectionStatus] = useState<
    "disconnected" | "connecting" | "connected" | null
  >(null);
  const [contentLoaded, setContentLoaded] = useState(false);
  const { documentStore, userStore } = useStore();

  const composerKey = useMemo(() => {
    return `${documentStore.selectedDocument?.workspaceId}-${documentStore.selectedDocument?.documentId}`;
  }, [documentStore.selectedDocument]);

  useEffect(() => {
    setYjsProvider(null);
    setConnectionStatus(null);
    setContentLoaded(false);
    setActiveUsers([]);
  }, [documentStore.selectedDocument]);

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

    return () => {
      yjsProvider.awareness.off("update", handleAwarenessUpdate);
    };
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
        }
      });

      provider.on("status", (s) => {
        setConnectionStatus(s.status);
        if (s.status === "connected") {
          notifications.show({
            title: "Preparing document",
            message: "Please wait while we prepare document's content",
            loading: true,
          });
        }
      });

      //@ts-ignore
      setTimeout(() => setYjsProvider(provider), 0);

      //@ts-ignore
      return provider;
    },
    []
  );

  const initialConfig = useMemo(
    () => ({
      namespace: `MyEditor-${composerKey}`,
      theme,
      onError: (error: Error) => console.log(error),
      nodes: [HeadingNode, ListNode, ListItemNode],
      editorState: null,
    }),
    [composerKey]
  );

  if (!documentStore.selectedDocument) {
    return null;
  }

  return (
    <>
      <DocumentHeader
        document={documentStore.selectedDocument}
        connectionStatus={
          connectionStatus !== "connected" && connectionStatus != null
            ? connectionStatus
            : contentLoaded
            ? "connected"
            : "connecting"
        }
      />
      <LexicalComposer key={composerKey} initialConfig={initialConfig}>
        <LoadingTrackerPlugin
          onFirstContentLoad={() => {
            notifications.clean();
            setContentLoaded(true);
          }}
        />
        <CollaborationPlugin
          id={`ws/${documentStore.selectedDocument.workspaceId}/${documentStore.selectedDocument.documentId}`}
          shouldBootstrap={false}
          providerFactory={createProvider}
          username={userStore.user!.username}
          cursorColor={avatarToColor.get(userStore.user!.avatar.charAt(81)!)}
        />
        <ToolbarPlugin
          myProfile={{
            name: userStore.user!.username,
            color: userStore.user!.avatar,
          }}
          otherUsers={activeUsers}
        />
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
});

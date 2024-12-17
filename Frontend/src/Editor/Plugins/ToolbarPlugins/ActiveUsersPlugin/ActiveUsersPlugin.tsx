import { Avatar, Tooltip, Group } from "@mantine/core";
import { ActiveUsersPluginInterface } from "../ToolbarPlugin";
import { UserProfile } from "../../../Interfaces/UserProfile";

export default function ActiveUsersPlugin({
  myProfile,
  otherUsers,
}: ActiveUsersPluginInterface) {
  function UserAvatar({
    user,
    isCurrentUser,
  }: {
    user: UserProfile;
    isCurrentUser: boolean;
  }) {
    return (
      <Tooltip
        label={`${user.name}${isCurrentUser ? " (You)" : ""}`}
        withArrow
        position="bottom"
      >
        <Avatar size="md" radius="xl" color={user.color}>
          {user.name[0].toUpperCase()}
        </Avatar>
      </Tooltip>
    );
  }

  return (
    <Group flex={1} justify="end">
      {otherUsers.map((user, index) => (
        <UserAvatar
          key={index}
          user={user}
          isCurrentUser={user.name === myProfile.name}
        />
      ))}
    </Group>
  );
}

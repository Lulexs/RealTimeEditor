import { Avatar, Tooltip, Group } from "@mantine/core";
import { ActiveUsersPluginInterface } from "../ToolbarPlugin";
import { UserProfile } from "../../../Interfaces/UserProfile";

const colorToAvatar = new Map(
  Object.entries({
    "#FF5733": 1,
    "#33FF57": 2,
    "#3357FF": 3,
    "#FFFF33": 4,
    "#FF33FF": 5,
    "#33FFFF": 6,
    "#FF9933": 7,
    "#9933FF": 8,
    "#33FF99": 9,
  })
);

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
        <Avatar
          size="md"
          radius="xl"
          src={`https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-${colorToAvatar.get(
            user.color
          )}.png`}
          color="white"
        >
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

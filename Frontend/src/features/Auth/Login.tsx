import { useForm } from "@mantine/form";
import {
  TextInput,
  PasswordInput,
  Button,
  Paper,
  Title,
  Text,
  Group,
  Stack,
  Box,
  Anchor,
  Flex,
} from "@mantine/core";
import { Link } from "react-router";
import { Home } from "lucide-react";

const Login = () => {
  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      username: "",
      password: "",
    },
    validate: {
      username: (value) =>
        value.length > 3 ? null : "Username must be at least 3 characters",
      password: (value) =>
        value.length > 6 ? null : "Password must be at least 6 characters",
    },
  });

  return (
    <Flex h="100vh">
      <Box mx="auto" mt="xl">
        <Paper radius="md" p="xl" withBorder>
          <Group justify="space-between" mb="md">
            <Title order={2}>Welcome back</Title>
            <Button
              component={Link}
              to="/"
              variant="subtle"
              leftSection={<Home size={20} />}
            >
              Home
            </Button>
          </Group>
          <form onSubmit={form.onSubmit((values) => console.log(values))}>
            <Stack>
              <TextInput
                withAsterisk
                label="Username"
                placeholder="Your username"
                key={form.key("username")}
                {...form.getInputProps("username")}
              />
              <PasswordInput
                withAsterisk
                label="Password"
                placeholder="Your password"
                key={form.key("password")}
                {...form.getInputProps("password")}
              />
            </Stack>
            <Group justify="space-between" mt="xl">
              <Text size="sm">
                Don't have an account?{" "}
                <Anchor component={Link} to="/register">
                  Register
                </Anchor>
              </Text>
              <Button type="submit">Login</Button>
            </Group>
          </form>
        </Paper>
      </Box>
    </Flex>
  );
};

export default Login;

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
  Select,
} from "@mantine/core";
import { Link } from "react-router";
import { Home } from "lucide-react";
import { useStore } from "../../app/stores/store";

const Login = () => {
  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      username: "",
      password: "",
      region: "",
    },
    validate: {
      username: (value) =>
        value.length > 3 ? null : "Username must be at least 3 characters",
      password: (value) =>
        value.length > 6 ? null : "Password must be at least 6 characters",
      region: (value) => (value.length > 1 ? null : "Please select a region"),
    },
  });

  const { userStore } = useStore();

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
          <form onSubmit={form.onSubmit((values) => userStore.login(values))}>
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
              <Select
                withAsterisk
                label="Region"
                placeholder="Select your region"
                data={[
                  "Aftica",
                  "Asia",
                  "Europe",
                  "North America",
                  "South America",
                  "Australia",
                  "Antarctica",
                ]}
                key={form.key("region")}
                {...form.getInputProps("region")}
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

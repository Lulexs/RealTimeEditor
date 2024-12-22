import { useState } from "react";
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
  SimpleGrid,
  Box,
  Anchor,
  Image,
  Flex,
  Select,
} from "@mantine/core";
import { Link } from "react-router";
import { Home } from "lucide-react";
import { useStore } from "../../app/stores/store";

const Register = () => {
  const [selectedAvatar, setSelectedAvatar] = useState(1);
  const { userStore } = useStore();

  const form = useForm({
    initialValues: {
      username: "",
      password: "",
      email: "",
      region: "",
    },
    validate: {
      username: (value) =>
        value.length < 3 ? "Username must be at least 3 characters" : null,
      password: (value) =>
        value.length < 6 ? "Password must be at least 6 characters" : null,
      email: (value) => (/^\S+@\S+$/.test(value) ? null : "Invalid email"),
      region: (value) => (!value ? "Region is required" : null),
    },
  });

  const handleSubmit = (values: any) => {
    const formData = {
      ...values,
      avatar: `https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-${selectedAvatar}.png`,
    };
    userStore.register(formData);
  };

  return (
    <Flex h="100vh">
      <Box maw={800} mx="auto" mt="xl">
        <Paper radius="md" p="xl" withBorder>
          <Group justify="space-between" mb="md">
            <Title order={2}>Create account</Title>
            <Button
              component={Link}
              to="/"
              variant="subtle"
              leftSection={<Home size={20} />}
            >
              Home
            </Button>
          </Group>

          <form onSubmit={form.onSubmit(handleSubmit)}>
            <Flex gap="xl">
              <Box style={{ flex: 1 }}>
                <Stack>
                  <TextInput
                    withAsterisk
                    label="Username"
                    placeholder="Your username"
                    error={form.errors.username}
                    {...form.getInputProps("username")}
                  />

                  <TextInput
                    withAsterisk
                    label="Email"
                    placeholder="your@email.com"
                    error={form.errors.email}
                    {...form.getInputProps("email")}
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

                  <PasswordInput
                    withAsterisk
                    label="Password"
                    placeholder="Your password"
                    error={form.errors.password}
                    {...form.getInputProps("password")}
                  />
                </Stack>
              </Box>

              <Box style={{ flex: 1 }}>
                <Stack>
                  <Text size="sm" fw={500}>
                    Choose your avatar
                  </Text>
                  <SimpleGrid cols={3}>
                    {[...Array(9)].map((_, index) => (
                      <Box
                        key={index + 1}
                        p="xs"
                        style={{
                          position: "relative",
                          cursor: "pointer",
                        }}
                      >
                        <Image
                          src={`https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-${
                            index + 1
                          }.png`}
                          styles={(theme) => ({
                            root: {
                              cursor: "pointer",
                              transition: "transform 0.2s ease",
                              border:
                                selectedAvatar === index + 1
                                  ? `3px solid ${theme.colors.blue[6]}`
                                  : "3px solid transparent",
                              boxShadow:
                                selectedAvatar === index + 1
                                  ? `0 0 10px ${theme.colors.blue[3]}`
                                  : "none",
                            },
                          })}
                          onClick={() => setSelectedAvatar(index + 1)}
                        />
                      </Box>
                    ))}
                  </SimpleGrid>
                </Stack>
              </Box>
            </Flex>

            <Group justify="space-between" mt="xl">
              <Text size="sm">
                Already have an account?{" "}
                <Anchor component={Link} to="/login">
                  Login
                </Anchor>
              </Text>
              <Button type="submit">Register</Button>
            </Group>
          </form>
        </Paper>
      </Box>
    </Flex>
  );
};

export default Register;

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
} from "@mantine/core";
import { Link } from "react-router";
import { Home } from "lucide-react";

const Register = () => {
  const [selectedAvatar, setSelectedAvatar] = useState(1);

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
    console.log("Register values:", formData);
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
                    styles={(theme) => ({
                      input: {
                        borderColor: form.errors.username
                          ? theme.colors.red[6]
                          : undefined,
                        "&:focus": {
                          borderColor: form.errors.username
                            ? theme.colors.red[6]
                            : undefined,
                        },
                      },
                    })}
                  />

                  <TextInput
                    withAsterisk
                    label="Email"
                    placeholder="your@email.com"
                    error={form.errors.email}
                    {...form.getInputProps("email")}
                    styles={(theme) => ({
                      input: {
                        borderColor: form.errors.email
                          ? theme.colors.red[6]
                          : undefined,
                        "&:focus": {
                          borderColor: form.errors.email
                            ? theme.colors.red[6]
                            : undefined,
                        },
                      },
                    })}
                  />

                  <TextInput
                    withAsterisk
                    label="Region"
                    placeholder="Your region"
                    error={form.errors.region}
                    {...form.getInputProps("region")}
                    styles={(theme) => ({
                      input: {
                        borderColor: form.errors.region
                          ? theme.colors.red[6]
                          : undefined,
                        "&:focus": {
                          borderColor: form.errors.region
                            ? theme.colors.red[6]
                            : undefined,
                        },
                      },
                    })}
                  />

                  <PasswordInput
                    withAsterisk
                    label="Password"
                    placeholder="Your password"
                    error={form.errors.password}
                    {...form.getInputProps("password")}
                    styles={(theme) => ({
                      input: {
                        borderColor: form.errors.password
                          ? theme.colors.red[6]
                          : undefined,
                        "&:focus": {
                          borderColor: form.errors.password
                            ? theme.colors.red[6]
                            : undefined,
                        },
                      },
                    })}
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

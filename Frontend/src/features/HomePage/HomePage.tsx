import {
  Button,
  Container,
  Text,
  Title,
  Group,
  Stack,
  Image,
} from "@mantine/core";
import styles from "./HomePage.module.css";
import hero from "../../assets/cover.gif";
import { useNavigate } from "react-router";

function HomePage() {
  const navigate = useNavigate();

  return (
    <Container size="md" py="xl" className={styles.container}>
      <Group mb="xl" className={styles.header}>
        <Title order={1} className={styles.logo}>
          CollabDocs
        </Title>
        <Group>
          <Button
            variant="outline"
            className={styles.button}
            onClick={() => navigate("/login")}
          >
            Login
          </Button>
          <Button
            className={styles.button}
            onClick={() => navigate("/register")}
          >
            Register
          </Button>
        </Group>
      </Group>

      <Stack align="center" gap="xl" className={styles.hero}>
        <Image
          src={hero}
          alt="Collaboration Illustration"
          radius="md"
          className={styles.heroImage}
        />
        <Title order={2} className={styles.heroTitle}>
          Collaborate in Real-Time
        </Title>
        <Text size="lg" c="dimmed" className={styles.heroText}>
          Create, edit, and share documents effortlessly with your team.
          Experience seamless real-time collaboration with powerful editing
          tools.
        </Text>
        <Button size="lg" className={styles.getStartedButton}>
          Get Started
        </Button>
      </Stack>

      <Stack mt="xl" gap="xl" align="center" className={styles.featuresSection}>
        <Title order={3} className={styles.featuresTitle}>
          Why Choose CollabDocs?
        </Title>
        <Group gap="xl" className={styles.featuresGroup}>
          <Stack className={styles.featureCard}>
            <Title order={4} className={styles.featureTitle}>
              Real-Time Editing
            </Title>
            <Text className={styles.featureText}>
              See changes instantly as your team collaborates on documents.
            </Text>
          </Stack>
          <Stack className={styles.featureCard}>
            <Title order={4} className={styles.featureTitle}>
              Easy to Use
            </Title>
            <Text className={styles.featureText}>
              Intuitive interface designed to boost productivity and ease
              workflow.
            </Text>
          </Stack>
        </Group>
      </Stack>
    </Container>
  );
}

export default HomePage;

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
import hero from "../assets/cover.gif";

function HomePage() {
  return (
    <Container size="md" py="xl" className={styles.container}>
      {/* Header Section */}
      <Group mb="xl" className={styles.header}>
        <Title order={1} className={styles.logo}>
          CollabDocs
        </Title>
        <Group>
          <Button variant="outline" className={styles.button}>
            Login
          </Button>
          <Button className={styles.button}>Register</Button>
        </Group>
      </Group>

      {/* Hero Section */}
      <Stack align="center" gap="xl" className={styles.hero}>
        <Image
          src={hero} // Replace with an appropriate image URL
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

      {/* Features Section */}
      <Stack mt="xl" gap="xl" className={styles.featuresSection}>
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
              Secure and Private
            </Title>
            <Text className={styles.featureText}>
              Your data is encrypted and protected with industry-leading
              security standards.
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

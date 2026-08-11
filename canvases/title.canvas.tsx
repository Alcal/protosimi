import {
  Card,
  CardBody,
  CardHeader,
  H1,
  Pill,
  Row,
  Spacer,
  Stack,
  Text,
  useHostTheme,
} from "cursor/canvas";

/**
 * Title screen graybox — Manos Limpias (1920×1080 intent).
 * Layout mock for design review; not final art.
 */
export default function TitleCanvas() {
  const theme = useHostTheme();

  return (
    <Stack gap={16} style={{ padding: 16 }}>
      <Stack gap={4}>
        <H1>Title — Manos Limpias</H1>
        <Text tone="secondary">
          Graybox mock · 1920×1080 · Rive will own final chrome
        </Text>
      </Stack>
      <Row gap={8}>
        <Pill tone="neutral" size="sm">
          Screen: Title
        </Pill>
        <Pill tone="info" size="sm">
          CTA: Jugar
        </Pill>
      </Row>
      <div
        style={{
          width: "100%",
          maxWidth: 960,
          aspectRatio: "16 / 9",
          background: theme.bg.chrome,
          border: `1px solid ${theme.stroke.tertiary}`,
          position: "relative",
          overflow: "hidden",
        }}
      >
        <div
          style={{
            position: "absolute",
            left: "50%",
            top: "38%",
            transform: "translate(-50%, -50%)",
            textAlign: "center",
            width: "70%",
          }}
        >
          <Text weight="semibold" style={{ color: theme.text.primary }}>
            Manos Limpias
          </Text>
          <Spacer height={8} />
          <Text tone="secondary" size="small">
            Hábitos Saludables con el Dr. Simi
          </Text>
        </div>
        <div
          style={{
            position: "absolute",
            left: "50%",
            bottom: "18%",
            transform: "translateX(-50%)",
            padding: "12px 36px",
            background: theme.accent.control,
            borderRadius: 4,
          }}
        >
          <Text weight="semibold" style={{ color: theme.text.onAccent }}>
            Jugar
          </Text>
        </div>
        <div
          style={{
            position: "absolute",
            left: 24,
            bottom: 24,
            width: 72,
            height: 72,
            borderRadius: "50%",
            border: `2px dashed ${theme.stroke.secondary}`,
            background: theme.fill.tertiary,
          }}
          title="Optional quiet host mark"
        />
      </div>
      <Card>
        <CardHeader>Intent</CardHeader>
        <CardBody>
          <Text>
            Establish brand before hygiene instruction. Single primary start
            action. No stage HUD on this screen.
          </Text>
        </CardBody>
      </Card>
    </Stack>
  );
}

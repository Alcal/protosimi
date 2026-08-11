import {
  Card,
  CardBody,
  CardHeader,
  H1,
  Pill,
  Row,
  Stack,
  Text,
  useHostTheme,
} from "cursor/canvas";

type IconState = "pending" | "active" | "complete";

const ICONS: { id: string; label: string; state: IconState }[] = [
  { id: "icon0", label: "Faucet", state: "complete" },
  { id: "icon1", label: "Wet/Rinse", state: "active" },
  { id: "icon2", label: "Soap", state: "pending" },
  { id: "icon3", label: "Towel", state: "pending" },
];

/**
 * Play HUD graybox — Manos Limpias (matches design sketch).
 * Top stage bar, right 4-icon column, bottom-left host, center play field.
 */
export default function PlayHudCanvas() {
  const theme = useHostTheme();

  function iconBorder(state: IconState): string {
    return state === "active" ? theme.accent.primary : theme.stroke.secondary;
  }

  return (
    <Stack gap={16} style={{ padding: 16 }}>
      <Stack gap={4}>
        <H1>Play HUD — Manos Limpias</H1>
        <Text tone="secondary">
          Graybox · 1920×1080 · Rive artboards AB_HUD_* own final layout
        </Text>
      </Stack>
      <Row gap={8} wrap>
        <Pill tone="neutral" size="sm">
          Stage bar 0–1
        </Pill>
        <Pill tone="info" size="sm">
          Right icon column
        </Pill>
        <Pill tone="success" size="sm">
          Host assist
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
            top: 16,
            left: 48,
            right: 96,
            height: 18,
            background: theme.fill.tertiary,
            borderRadius: 4,
            overflow: "hidden",
          }}
        >
          <div
            style={{
              width: "42%",
              height: "100%",
              background: theme.accent.primary,
            }}
          />
        </div>

        <div
          style={{
            position: "absolute",
            top: 56,
            right: 16,
            width: 56,
            display: "flex",
            flexDirection: "column",
            gap: 10,
          }}
        >
          {ICONS.map((icon) => (
            <div
              key={icon.id}
              title={icon.label}
              style={{
                width: 56,
                height: 56,
                border: `2px solid ${iconBorder(icon.state)}`,
                background: theme.bg.elevated,
                position: "relative",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <Text size="small" tone="secondary">
                {icon.label.slice(0, 6)}
              </Text>
              {icon.state === "complete" ? (
                <div
                  style={{
                    position: "absolute",
                    top: -4,
                    right: -4,
                    width: 16,
                    height: 16,
                    borderRadius: "50%",
                    background: theme.accent.primary,
                  }}
                />
              ) : null}
            </div>
          ))}
        </div>

        <div
          style={{
            position: "absolute",
            left: "18%",
            right: "16%",
            top: "22%",
            bottom: "18%",
          }}
        >
          <PropBox theme={theme} left="42%" top="0%" label="Faucet" />
          <PropBox theme={theme} left="0%" top="40%" label="Soap" />
          <PropBox theme={theme} left="78%" top="40%" label="Towel" />
          <div
            style={{
              position: "absolute",
              left: "28%",
              top: "38%",
              width: "44%",
              height: "42%",
              border: `2px solid ${theme.stroke.primary}`,
              borderRadius: 12,
              background: theme.fill.tertiary,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <Text size="small" weight="semibold">
              Hands (FP)
            </Text>
          </div>
        </div>

        <div
          style={{
            position: "absolute",
            left: 20,
            bottom: 20,
            width: 88,
            height: 88,
            borderRadius: "50%",
            border: `2px solid ${theme.stroke.primary}`,
            background: theme.fill.tertiary,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          <Text size="small" tone="secondary">
            Host
          </Text>
        </div>
      </div>

      <Card>
        <CardHeader>Layout contract</CardHeader>
        <CardBody>
          <Stack gap={8}>
            <Text>
              Top: current-stage progress. Right: four icons (faucet, wet/rinse,
              soap, towel) with pending / active / complete. Bottom-left: host
              portrait for VO and hijack assist. Center: first-person hands and
              graybox props; camera eases toward active focus.
            </Text>
            <Text tone="secondary">
              Example state shown: stage 1 Wet Hands active; faucet icon
              already complete.
            </Text>
          </Stack>
        </CardBody>
      </Card>
    </Stack>
  );
}

function PropBox(props: {
  theme: ReturnType<typeof useHostTheme>;
  left: string;
  top: string;
  label: string;
}) {
  const { theme, left, top, label } = props;
  return (
    <div
      style={{
        position: "absolute",
        left,
        top,
        width: 64,
        height: 40,
        border: `1px dashed ${theme.stroke.secondary}`,
        background: theme.bg.elevated,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      <Text size="small" tone="secondary">
        {label}
      </Text>
    </div>
  );
}

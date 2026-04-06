import { useEffect } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { defaultOpsRoute, useAuth } from "../../app/contexts/AuthContext";
import { useToast } from "../../app/contexts/ToastContext";
import { AlertBanner } from "../../components/ui/AlertBanner";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { MetricCard } from "../../components/ui/MetricCard";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { fetchCentralHealth, fetchEdgeHealth, login } from "../../lib/api";
import { formatDateTime } from "../../lib/format";
import { queryKeys } from "../../lib/queryKeys";

const loginSchema = z.object({
  email: z.string().trim().email("Enter a valid staff email."),
  password: z.string().min(8, "Enter your password."),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export function OpsLoginPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { session, signIn } = useAuth();
  const { pushToast } = useToast();
  const redirectTarget = searchParams.get("redirect");

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const centralHealthQuery = useQuery({
    queryKey: queryKeys.centralHealth,
    queryFn: fetchCentralHealth,
  });

  const edgeHealthQuery = useQuery({
    queryKey: queryKeys.edgeHealth,
    queryFn: fetchEdgeHealth,
  });

  const loginMutation = useMutation({
    mutationFn: ({ email, password }: LoginFormValues) => login(email.trim(), password),
    onSuccess: (response) => {
      signIn(response);
      pushToast({
        tone: "success",
        title: "Signed in",
        description: "The operations workspace is ready.",
      });

      navigate(redirectTarget ? decodeURIComponent(redirectTarget) : defaultOpsRoute(response.user.roles), { replace: true });
    },
    onError: (error) => {
      pushToast({
        tone: "danger",
        title: "Sign-in failed",
        description: error instanceof Error ? error.message : "Invalid staff credentials.",
      });
    },
  });

  useEffect(() => {
    if (!session) {
      return;
    }

    navigate(defaultOpsRoute(session.user.roles), { replace: true });
  }, [navigate, session]);

  return (
    <div className="login-layout">
      <section className="hero-panel ops-hero">
        <div className="hero-copy-block">
          <SectionHeading
            eyebrow="Operations access"
            title="A faster, clearer control center for rangers and administrators."
            description="The new operations workspace surfaces connection health, edge readiness, and high-frequency actions without burying them in stacked demo forms."
          />
          <div className="metric-grid compact-grid">
            <MetricCard
              label="Central API"
              value={centralHealthQuery.data?.status ?? "Checking"}
              detail={centralHealthQuery.data ? `Verified ${formatDateTime(centralHealthQuery.data.checkedAtUtc)}` : "Waiting for status"}
              tone="accent"
            />
            <MetricCard
              label="Edge node"
              value={edgeHealthQuery.data?.status ?? "Checking"}
              detail={edgeHealthQuery.data ? `Verified ${formatDateTime(edgeHealthQuery.data.checkedAtUtc)}` : "Waiting for status"}
            />
          </div>
        </div>

        <Card className="login-card">
          <SectionHeading eyebrow="Staff sign-in" title="Authenticate to continue" />
          {loginMutation.isError ? (
            <AlertBanner title="Login problem" tone="danger">
              {loginMutation.error instanceof Error ? loginMutation.error.message : "Sign-in failed."}
            </AlertBanner>
          ) : null}

          <form
            className="field-stack"
            onSubmit={form.handleSubmit(async (values) => {
              await loginMutation.mutateAsync(values);
            })}
          >
            <label className="field-label">
              <span>Email</span>
              <input {...form.register("email")} autoComplete="username" placeholder="you@park.local" type="email" />
              {form.formState.errors.email ? <small className="field-error">{form.formState.errors.email.message}</small> : null}
            </label>

            <label className="field-label">
              <span>Password</span>
              <input {...form.register("password")} autoComplete="current-password" placeholder="Enter password" type="password" />
              {form.formState.errors.password ? <small className="field-error">{form.formState.errors.password.message}</small> : null}
            </label>

            <Button busy={loginMutation.isPending} fullWidth tone="primary" type="submit">
              Sign in to operations
            </Button>
          </form>

          <Link className="text-link" to="/">
            Return to public booking
          </Link>
        </Card>
      </section>
    </div>
  );
}

import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Card } from "../../components/ui/Card";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { getLastReservationId } from "../../lib/session";

export function PaymentReturnPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  useEffect(() => {
    const reservationId = getLastReservationId();
    const query = searchParams.toString();
    const nextUrl = reservationId ? `/reservation/${reservationId}${query ? `?${query}` : ""}` : `/book${query ? `?${query}` : ""}`;

    const timer = window.setTimeout(() => {
      navigate(nextUrl, { replace: true });
    }, 700);

    return () => window.clearTimeout(timer);
  }, [navigate, searchParams]);

  return (
    <Card>
      <SectionHeading eyebrow="Returning from payment" title="Reconnecting you to the latest reservation status." />
      <LoadingSkeleton lines={3} />
    </Card>
  );
}


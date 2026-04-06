type Props = {
  lines?: number;
};

export function LoadingSkeleton({ lines = 3 }: Props) {
  return (
    <div className="skeleton-stack" aria-hidden="true">
      {Array.from({ length: lines }).map((_, index) => (
        <span key={index} className="skeleton-line" />
      ))}
    </div>
  );
}


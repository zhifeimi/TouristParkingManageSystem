type Step = {
  id: string;
  label: string;
  description: string;
};

type Props = {
  currentStepId: string;
  steps: Step[];
};

export function StepIndicator({ currentStepId, steps }: Props) {
  const currentStepIndex = steps.findIndex((step) => step.id === currentStepId);

  return (
    <ol className="step-indicator" aria-label="Booking steps">
      {steps.map((step, index) => {
        const status = index < currentStepIndex ? "done" : index === currentStepIndex ? "current" : "upcoming";
        return (
          <li key={step.id} className={`step-item step-${status}`}>
            <span className="step-dot" aria-hidden="true" />
            <div>
              <strong>{step.label}</strong>
              <span>{step.description}</span>
            </div>
          </li>
        );
      })}
    </ol>
  );
}


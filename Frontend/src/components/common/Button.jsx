const BASE_CLASSES = "flex items-center justify-center w-full gap-2 py-2 text-sm font-medium rounded-full transition disabled:opacity-40 disabled:cursor-not-allowed bg-gray-900 text-white hover:bg-gray-800 dark:bg-gray-200 dark:text-black dark:hover:bg-white"

const Button = ({ children, loading, disabled, onClick, type = "button", className = "" }) => {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={loading || disabled}
      className={`${BASE_CLASSES} ${className}`}
    >
      {children}
    </button>
  );
};

export default Button;
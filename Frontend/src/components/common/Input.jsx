const BASE_CLASSES = "w-full px-6 py-2 text-sm font-medium rounded-xl outline-none transition disabled:opacity-40 disabled:cursor-not-allowed bg-white text-gray-900  placeholder-gray-400 focus:border-2 focus:border-gray-900  focus:bg-white  dark:bg-white/10 dark:text-white dark:placeholder-white/40 dark:focus:border-white/40 "

const Input = ({ placeholder, disabled, onChange, value, type = "text", className = "" }) => {
  return (
    <input
      type={type}
      value={value}
      onChange={onChange}
      disabled={disabled}
      placeholder={placeholder}
      className={`${BASE_CLASSES} ${className}`}
    />
  );
};

export default Input;
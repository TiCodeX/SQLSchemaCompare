const eslint = require("@eslint/js");
const tseslint = require("typescript-eslint");
const pluginOnlyError = require("eslint-plugin-only-error");

module.exports = tseslint.config(
  {
    plugins: {
      pluginOnlyError,
    },
  },
  eslint.configs.recommended,
  {
    languageOptions: {
      parserOptions: {
        project: true,
        tsconfigRootDir: __dirname,
      },
    },
    files: ["**/*.ts"],
  },
  ...tseslint.configs.recommendedTypeChecked
);

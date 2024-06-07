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
        tsconfigDirName: __dirname,
      },
    },
    files: ["**/*.ts"],
  },
  {
    ignores: ["wwwroot/lib"],
  },
  ...tseslint.configs.recommendedTypeChecked
);

const tseslint = require("typescript-eslint");
const baseConfig = require("../BaseEslintConfig.js").getBaseConfig(__dirname);

module.exports = tseslint.config(
  {
    ignores: [
      "**/wwwroot/**/*.js",
      "**/wwwroot/lib",
    ],
  },
  {
    ...baseConfig,
    rules: {
      ...baseConfig.rules,
      "sonarjs/new-cap": "off", // Too many changes to fix, is it worth it?
      "unicorn/filename-case": ["error", { "case": "pascalCase" }],
    },
  },
);

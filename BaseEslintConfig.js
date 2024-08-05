module.exports = {
  getBaseConfig: (dir) => {
    const eslint = require(dir + "/node_modules/@eslint/js");
    const tseslint = require(dir + "/node_modules/typescript-eslint/dist");
    const pluginOnlyError = require(dir + "/node_modules/eslint-plugin-only-error");

    return {
      files: ["**/*.ts"],
      extends: [
        eslint.configs.recommended,
        ...tseslint.configs.recommended,
        ...tseslint.configs.recommendedTypeChecked,
      ],
      plugins: {
        pluginOnlyError,
      },
      languageOptions: {
        parser: tseslint.parser,
        parserOptions: {
          project: true,
          tsconfigRootDir: __dirname,
        },
      },
    };
  },
};

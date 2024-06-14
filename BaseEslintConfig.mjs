// @ts-check
import url from "node:url";
import path from "node:path";

export const getBaseConfig = async (dirname) => {
  const fileUrl = url.pathToFileURL(path.join(dirname, "/"));
  const eslint = (await import(import.meta.resolve("@eslint/js", fileUrl))).default;
  const tseslint = (await import(import.meta.resolve("typescript-eslint", fileUrl))).default;

  const deprecationPlugin = (await import(import.meta.resolve("eslint-plugin-deprecation", fileUrl))).default;
  const eslintCommentsPlugin = (await import(import.meta.resolve("eslint-plugin-eslint-comments", fileUrl))).default;
  const importPlugin = (await import(import.meta.resolve("eslint-plugin-import", fileUrl))).default;
  const jsdocPlugin = (await import(import.meta.resolve("eslint-plugin-jsdoc", fileUrl))).default;
  const simpleImportSortPlugin = (await import(import.meta.resolve("eslint-plugin-simple-import-sort", fileUrl))).default;
  const unicornPlugin = (await import(import.meta.resolve("eslint-plugin-unicorn", fileUrl))).default;
  const sonarjsPlugin = (await import(import.meta.resolve("eslint-plugin-sonarjs", fileUrl))).default;
  const onlyErrorPlugin = (await import(import.meta.resolve("eslint-plugin-only-error", fileUrl))).default;

  return [
    // register all of the plugins up-front
    {
      // note - intentionally uses computed syntax to make it easy to sort the keys
      name: "plugins",
      plugins: {
        ["@typescript-eslint"]: tseslint.plugin,
        ["deprecation"]: deprecationPlugin,
        ["eslint-comments"]: eslintCommentsPlugin,
        ["import"]: importPlugin,
        ["jsdoc"]: jsdocPlugin,
        ["simple-import-sort"]: simpleImportSortPlugin,
        ["unicorn"]: unicornPlugin,
        ["sonarjs"]: sonarjsPlugin,
        ["only-error"]: onlyErrorPlugin,
      },
    },
    {
      name: "global-ignores",
      ignores: [
        "**/node_modules/**",
      ],
    },

    // extends ...
    {
      name: "eslint/recommended",
      ...eslint.configs.recommended,
    },
    //eslint.configs.all, ???
    ...tseslint.configs.recommendedTypeChecked,
    ////{
    ////  name: "eslint-plugin-deprecation/recommended",
    ////  rules: {
    ////    ...deprecationPlugin.configs.recommended.rules,
    ////  },
    ////},
    {
      name: "eslint-plugin-eslint-comments/recommended",
      rules: {
        ...eslintCommentsPlugin.configs.recommended.rules,
      },
    },
    //importPlugin???
    jsdocPlugin.configs["flat/recommended-typescript-error"],
    //simpleImportSortPlugin???
    {
      name: "eslint-plugin-unicorn/recommended",
      rules: {
        ...unicornPlugin.configs["flat/recommended"].rules,
      },
    },
    {
      name: "eslint-plugin-sonarjs/recommended",
      rules: {
        ...sonarjsPlugin.configs.recommended.rules,
      },
    },

    // base config
    {
      name: "base-config",
      languageOptions: {
        parser: tseslint.parser,
        parserOptions: {
          project: true,
          tsconfigRootDir: dirname,
        },
      },
      rules: {
        //// make sure we"re not leveraging any deprecated APIs
        //"deprecation/deprecation": "error",

        //  // TODO: https://github.com/typescript-eslint/typescript-eslint/issues/8538
        //  "@typescript-eslint/no-confusing-void-expression": "off",

        //
        // eslint-base
        //

        //  curly: ["error", "all"],
        //  eqeqeq: [
        //    "error",
        //    "always",
        //    {
        //      null: "never",
        //    },
        //  ],
        //  "logical-assignment-operators": "error",
        //  "no-else-return": "error",
        //  "no-mixed-operators": "error",
        //  "no-console": "error",
        //  "no-process-exit": "error",
        //  "no-fallthrough": [
        //    "error",
        //    { commentPattern: ".*intentional fallthrough.*" },
        //  ],
        //  "one-var": ["error", "never"],

        //  //
        //  // eslint-plugin-eslint-comment
        //  //

        //  // require a eslint-enable comment for every eslint-disable comment
        //  "eslint-comments/disable-enable-pair": [
        //    "error",
        //    {
        //      allowWholeFile: true,
        //    },
        //  ],
        //  // disallow a eslint-enable comment for multiple eslint-disable comments
        //  "eslint-comments/no-aggregating-enable": "error",
        //  // disallow duplicate eslint-disable comments
        //  "eslint-comments/no-duplicate-disable": "error",
        //  // disallow eslint-disable comments without rule names
        //  "eslint-comments/no-unlimited-disable": "error",
        //  // disallow unused eslint-disable comments
        //  "eslint-comments/no-unused-disable": "error",
        //  // disallow unused eslint-enable comments
        //  "eslint-comments/no-unused-enable": "error",
        //  // disallow ESLint directive-comments
        //  "eslint-comments/no-use": [
        //    "error",
        //    {
        //      allow: [
        //        "eslint-disable",
        //        "eslint-disable-line",
        //        "eslint-disable-next-line",
        //        "eslint-enable",
        //        "global",
        //      ],
        //    },
        //  ],

        //
        // eslint-plugin-import
        //
        //  // enforces consistent type specifier style for named imports
        //  "import/consistent-type-specifier-style": "error",
        //  // disallow non-import statements appearing before import statements
        //  "import/first": "error",
        //  // Require a newline after the last import/require in a group
        //  "import/newline-after-import": "error",
        //  // Forbid import of modules using absolute paths
        //  "import/no-absolute-path": "error",
        //  // disallow AMD require/define
        //  "import/no-amd": "error",
        //  // forbid default exports - we want to standardize on named exports so that imported names are consistent
        //  "import/no-default-export": "error",
        //  // disallow imports from duplicate paths
        //  "import/no-duplicates": "error",
        //  // Forbid the use of extraneous packages
        //  "import/no-extraneous-dependencies": [
        //    "error",
        //    {
        //      devDependencies: true,
        //      peerDependencies: true,
        //      optionalDependencies: false,
        //    },
        //  ],
        //  // Forbid mutable exports
        //  "import/no-mutable-exports": "error",
        //  // Prevent importing the default as if it were named
        //  "import/no-named-default": "error",
        //  // Prohibit named exports
        //  "import/no-named-export": "off", // we want everything to be a named export
        //  // Forbid a module from importing itself
        //  "import/no-self-import": "error",
        //  // Require modules with a single export to use a default export
        //  "import/prefer-default-export": "off", // we want everything to be named

        //  // enforce a sort order across the codebase
        //  "simple-import-sort/imports": "error",

        //  //
        //  // eslint-plugin-jsdoc
        //  //

        //  // We often use @remarks or other ad-hoc tag names
        //  "jsdoc/check-tag-names": "off",
        //  // https://github.com/gajus/eslint-plugin-jsdoc/issues/1169
        //  "jsdoc/check-param-names": "off",
        //  // https://github.com/gajus/eslint-plugin-jsdoc/issues/1175
        //  "jsdoc/require-jsdoc": "off",
        //  "jsdoc/require-param": "off",
        //  "jsdoc/require-returns": "off",
        //  "jsdoc/require-yields": "off",
        //  "jsdoc/tag-lines": "off",
        //  "jsdoc/informative-docs": "error",

        //
        // eslint-plugin-unicorn
        //
        "unicorn/no-array-for-each": "off",
        "unicorn/prefer-module": "off",
        "unicorn/prevent-abbreviations": "off",
        "unicorn/switch-case-braces": "off",
        "unicorn/throw-new-error": "off",

        //
        // eslint-plugin-sonarjs
        //
        "sonarjs/no-duplicate-string": "off",
      },
    },
    {
      name: "no-ts-specific-rules",
      files: ["**/*.{js,cjs,mjs}"],
      extends: [tseslint.configs.disableTypeChecked],
      rules: {
        //// turn off other type-aware rules
        //"deprecation/deprecation": "off",
        //"@typescript-eslint/internal/no-poorly-typed-ts-props": "off",

        //// turn off rules that don"t apply to JS code
        //"@typescript-eslint/explicit-function-return-type": "off",
      },
    },
    {
      name: "no-default-export",
      files: [
        "eslint.config.{js,cjs,mjs}",
        "**/*.d.{ts,tsx,cts,mts}",
      ],
      rules: {
        "import/no-default-export": "off",
      },
    },
  ];
  //return [{
  //  name: "eslint/recommended",
  //  files: ["**/*.js", "**/*.jsx"],
  //  ...eslint.configs.recommended,
  //},
  //...tseslint.config({
  //  files: ["**/*.ts", "**/*.tsx"],
  //},
  //  ...tseslint.configs.recommended)];
};

//export { getBaseConfig };

//{
//files: ["**/*.ts", "**/*.tsx"],
//extends: [
//  ,
//  pluginUnicorn.configs["flat/recommended"],
//  pluginJsDoc.configs["flat/recommended-typescript"],
//],
//plugins: {
//  pluginSonarJS,
//  pluginUnicorn,
//  pluginJsDoc,
//  pluginOnlyError,
//},
//languageOptions: {
//  parser: tseslint.parser,
//    parserOptions: {
//    project: true,
//      tsconfigRootDir: __dirname,
//    },
//},
//rules: {
//  // To keep disabled?
//  "unicorn/throw-new-error": "off",
//  "unicorn/prefer-module": "off",
//  "unicorn/switch-case-braces": "off",
//  "sonarjs/no-duplicate-string": "off",
//  "unicorn/prevent-abbreviations": "off",
//  "unicorn/no-array-for-each": "off",
//}
//});
//  }
//};

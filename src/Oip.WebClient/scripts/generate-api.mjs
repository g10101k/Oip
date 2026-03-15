import fs from "node:fs";
import path from "node:path";
import {generateApi, generateTemplates} from "swagger-typescript-api";
import {ArgumentParser} from 'argparse';

const parser = new ArgumentParser({
  description: 'Argparse example'
});

parser.add_argument('-o', '--output', {help: 'Output path'});
parser.add_argument('-i', '--input', {help: 'Input swagger file path'});
parser.add_argument('-t', '--templates', {help: 'Templates'});
let a = parser.parse_args();

/* NOTE: all fields are optional expect one of `input`, `url`, `spec` */

let config = {
  input: path.resolve(process.cwd(), a.input),
  templates: path.resolve(process.cwd(), a.templates),
  httpClientType: "fetch", // or "fetch"
  defaultResponseAsSuccess: false,
  generateClient: true,
  generateRouteTypes: false,
  generateResponses: true,
  toJS: false,
  extractRequestParams: true,
  extractRequestBody: true,
  extractEnums: true,
  unwrapResponseData: true,
  modular: true,
  prettier: {
    printWidth: 120,
    tabWidth: 2,
    trailingComma: "all",
    parser: "typescript",
  },
  defaultResponseType: "void",
  singleHttpClient: true,
  cleanOutput: false,
  enumNamesAsValues: false,
  moduleNameFirstTag: true,
  generateUnionEnums: false,
  dataContractPrefix: "test-",
  typePrefix: "",
  typeSuffix: "",
  enumKeyPrefix: "",
  enumKeySuffix: "",
  addReadonly: false,
  sortTypes: false,
  sortRouters: false,
  extractingOptions: {
    requestBodySuffix: ["Payload", "Body", "Input"],
    requestParamsSuffix: ["Params"],
    responseBodySuffix: ["Data", "Result", "Output"],
    responseErrorSuffix: [
      "Error",
      "Fail",
      "Fails",
      "ErrorData",
      "HttpError",
      "BadResponse",
    ],
  },
  /** allow to generate extra files based with this extra templates, see more below */
  extraTemplates: [],
  anotherArrayType: false,
  fixInvalidTypeNamePrefix: "Type",
  fixInvalidEnumKeyPrefix: "Value",
  codeGenConstructs: (constructs) => ({
    ...constructs,
    RecordType: (key, value) => `MyRecord<key, value>`,
  }),
  primitiveTypeConstructs: (constructs) => ({
    ...constructs,
    string: {
      "date-time": "Date",
    },
  }),
  hooks: {
    onCreateComponent: (component) => {
    },
    onCreateRequestParams: (rawType) => {
    },
    onCreateRoute: (routeData) => {
    },
    onCreateRouteName: (routeNameInfo, rawRouteInfo) => {
      if (routeNameInfo.usage.startsWith(rawRouteInfo.moduleName)) {
        const str = routeNameInfo.usage.substring(rawRouteInfo.moduleName.length);
        routeNameInfo.usage = str[0].toLowerCase() + str.slice(1);
        routeNameInfo.original = routeNameInfo.usage;
      }
      return routeNameInfo;
    },
    onFormatRouteName: (routeInfo, templateRouteName) => {
    },
    onFormatTypeName: (typeName, rawTypeName, schemaType) => {
    },
    onInit: (configuration) => {
    },
    onPreParseSchema: (originalSchema, typeName, schemaType) => {
    },
    onParseSchema: (originalSchema, parsedSchema) => {
    },
    onPrepareConfig: (currentConfiguration) => {
    },
  },
}
const toKebabCase = (str) =>
  str &&
  str
    .match(/[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+/g)
    .map(x => x.toLowerCase())
    .join('-');

generateApi(config)
  .then(async ({files, configuration}) => {
    console.log(files);
    for (const f of files) {
      if (f.fileContent) {
        let dir = path.join(process.cwd(), a.output);
        if (!fs.existsSync(dir))
          fs.mkdirSync(dir);

        if (f.fileName === 'data-contracts')
          f.fileName = config.dataContractPrefix + f.fileName;

        const absolutePath = path.join(dir, `${toKebabCase(f.fileName)}${f.fileExtension}`);
        fs.writeFile(absolutePath, f.fileContent, (err) => {
          if (err) {
            console.log(err);
          } else {
            console.log(`File create: ${f.fileName}${f.fileExtension}`);
          }
        });
      }
    }
  })
  .catch((e) => console.error(e));

/*
generateTemplates({
  cleanOutput: false,
  output: './output/',
  httpClientType: "fetch",
  modular: true,
  silent: false,
  rewrite: false,
});
*/

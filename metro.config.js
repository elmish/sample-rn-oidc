const path = require('path');
const { getDefaultConfig } = require('expo/metro-config');

const config = getDefaultConfig(__dirname);

// Include Fable output directory
config.watchFolders = [__dirname + '/out'];

// Stub unused optional Fable.ReactNative dependencies
const emptyShim = path.resolve(__dirname, 'shims/empty.js');
const stubModules = [
  '@react-native-community/netinfo',
  'react-native-camera',
  'react-native-dialog',
  'react-native-fs',
  'react-native-image-picker',
  'react-native-image-resizer',
  'react-native-maps',
  'react-native-modal-datetime-picker',
  'react-native-popup-menu',
  'react-native-signature-view',
  'react-native-sqlite-storage',
  'react-native-video',
];

config.resolver.extraNodeModules = {
  buffer: require.resolve('buffer/'),
  ...Object.fromEntries(stubModules.map(m => [m, emptyShim])),
};

module.exports = config;

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import React from 'react';
import '../../styles/LoadingSpinner.css';

const LoadingSpinner: React.FC = () => {
  return (
    <div className="loader-overlay">
      <div className="loader"></div>
      <div className="loader-text">Loading please wait...</div>
    </div>
  );
};

export default LoadingSpinner;

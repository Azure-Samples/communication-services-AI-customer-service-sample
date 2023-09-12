// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import React, { useState } from 'react';
import '../../styles/Assistant.css';

type SectionProps = {
  title: string;
  details: string;
};

const Section: React.FC<SectionProps> = ({ title, details }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="section">
      <h2 onClick={() => setIsOpen(!isOpen)}>
        <span className={`bullet ${isOpen ? 'open' : ''}`}>&gt;</span>
        {title}
      </h2>
      {isOpen && <p>{details}</p>}
    </div>
  );
};

export default Section;
